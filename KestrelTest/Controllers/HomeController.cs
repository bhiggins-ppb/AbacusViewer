using Microsoft.AspNetCore.Mvc;
using AbacusViewer.Models;
using Lightyear.Common.Agglomerator.Contracts.Proto.PesV3;
using Newtonsoft.Json;
using Confluent.Kafka;
using System.IO.Compression;
using System.Text;

namespace AbacusViewer.Controllers
{
    public partial class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Markets()
        {
            return View();
        }

        public ActionResult Selections(long eventId, string filter)
        {
            return View(new SelectionFilter
            {
                EventId = eventId,
                Filter = filter
            });
        }

        [HttpPost]
        public ActionResult Selections(long eventId, int current, int rowCount, Dictionary<string, string> sort, string searchPhrase, string filter)
        {
            if (searchPhrase == null) searchPhrase = string.Empty;

            var raw = GetRawSelections(eventId)?.ToList();

            var newFiltered = new List<AbacusSelection>();

            var filtered = raw.Where(x => x.MarketTypeId.ToString().IndexOf(searchPhrase, StringComparison.OrdinalIgnoreCase) > -1
                                          || x.MarketName.ToString().IndexOf(searchPhrase, StringComparison.OrdinalIgnoreCase) > -1
                                          || x.SelectionId.ToString().IndexOf(searchPhrase, StringComparison.OrdinalIgnoreCase) > -1);

            if (sort != null)
            {
                if (sort.ContainsKey("market_type_id")) filtered = (sort["market_type_id"] != "desc") ? filtered.OrderBy(x => x.MarketTypeId) : filtered.OrderByDescending(x => x.MarketTypeId);
                else if (sort.ContainsKey("market_name")) filtered = (sort["market_name"] != "desc") ? filtered.OrderBy(x => x.MarketName) : filtered.OrderByDescending(x => x.MarketName);
                else if (sort.ContainsKey("selection_id")) filtered = (sort["selection_id"] != "desc") ? filtered.OrderBy(x => x.SelectionId) : filtered.OrderByDescending(x => x.SelectionId);
                else filtered = filtered.OrderBy(x => x.MarketName);
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var markets = filter.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var rec in filtered)
                {
                    var isInMarket = false;
                    foreach (var market in markets)
                    {
                        if (rec.MarketName.ToLower().Contains(market.ToLower()))
                        {
                            isInMarket = true;
                        }
                    }

                    if (isInMarket)
                    {
                        newFiltered.Add(rec);
                    }
                }
            }
            else
            {
                newFiltered = filtered.ToList();
            }

            var paged = newFiltered.Skip((current - 1) * rowCount).Take(rowCount > -1 ? rowCount : int.MaxValue);

            string json = "{\"current\": " + current + ", \"rowCount\": " + rowCount + ",\"rows\": " + JsonConvert.SerializeObject(paged.ToList()) + ", \"total\": " + filtered.Count() + "}";
            return new ContentResult { Content = json, ContentType = "application/json" };
        }

        private IEnumerable<AbacusSelection> GetRawSelections(long currentEventId)
        {
            PublisherMessage message = GetMostRecentMessageForEventId(currentEventId);

            AbacusEventFromPublisherMessage evt = new AbacusEventFromPublisherMessage(message);
            return evt?.Selections;
        }

        public static PublisherMessage GetMostRecentMessageForEventId(long desiredEventId)
        {
            // There appears to be a constant stream of messages with event id 0 (why?) on partition 0,
            // so any event id that ends with 0 is good for confirming that the consume loop works.
            List<TopicPartition> topicPartitions = GetTopicPartitions();

            // The message should be on the partition whose value is the modulus of the event id that we're looking for
            TopicPartition relevantTopicPartition = topicPartitions.Single(tp => tp.Partition.Value == desiredEventId % topicPartitions.Count);

            using var consumer = new ConsumerBuilder<Ignore, byte[]>(new ConsumerConfig()
            {
                BootstrapServers = "b-6.socmsk-nxt.dztv1a.c4.kafka.eu-west-1.amazonaws.com:9092", //kafkaConsumerConfiguration.BrokerList,
                GroupId = "test-CG128", //kafkaConsumerConfiguration.ConsumerGroup,

                AutoOffsetReset = AutoOffsetReset.Earliest,
                AutoCommitIntervalMs = 10_000,
            }).Build();

            // Get the current low (oldest) and high (newest) message offsets for this partition
            var watermarkOffsets = consumer.QueryWatermarkOffsets(relevantTopicPartition, TimeSpan.FromSeconds(40));
            Console.WriteLine($"{relevantTopicPartition}: Low-{watermarkOffsets.Low.Value}, High-{watermarkOffsets.High.Value}");

            // Assign the TPO to the consumer
            consumer.Assign(new TopicPartitionOffset(relevantTopicPartition, watermarkOffsets.High));

            // The high offset appears to be one past the most recent message
            long offsetValue = watermarkOffsets.High.Value;
            ConsumeResult<Ignore, byte[]> result;
            var publishTime = DateTime.UtcNow;

            PublisherMessage message = null;
            // Note: This will always consume at least the most recent message, even if it might from before the window of interest
            do
            {
                // Seek to the next older message offset - first time through the loop this will move the offset to the newest message
                consumer.Seek(new TopicPartitionOffset(relevantTopicPartition, new Offset(--offsetValue)));

                // Attempt to consume, wait at most 5 seconds for a response
                result = consumer.Consume(15000);
                if (result != null)
                {
                    // Extract details from headers
                    long eventId = long.Parse(Encoding.Default.GetString(result.Message.Headers.Single(h => h.Key == "ppEventId").GetValueBytes()));
                    publishTime = FromMillisSinceEpoch(long.Parse(Encoding.Default.GetString(result.Message.Headers.Single(h => h.Key == "ppLyKafkaPublishTime").GetValueBytes())));
                    Console.WriteLine($"Partition: {result.TopicPartition.Partition.Value}, Offset: {result.Offset.Value}, Event Id: {eventId}, Length: {result.Message.Value.Length}, Publish time: {publishTime}");

                    if (eventId == desiredEventId)
                    {
                        Console.WriteLine("I've finally found what I've been looking for!");

                        // TODO: Extract message body
                        using var ms = new MemoryStream(result.Message.Value);
                        //File.WriteAllBytes("C:\\Work\\Sports\\Soccer\\"+eventId.ToString(), result.Message.Value);
                        using var gzdec = new GZipStream(ms, CompressionMode.Decompress);

                        message = PublisherMessage.Parser.ParseFrom(gzdec);

                        break;
                    }
                }

                // Exit the loop if we received no result; we're far enough back in time; or we've just read the oldest message in the queue
            } while (result != null && publishTime > DateTime.UtcNow.AddMinutes(-120) && offsetValue > watermarkOffsets.Low.Value);

            // Unassign and close the consumer

            consumer.Unassign();
            consumer.Close();

            return message;
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime FromMillisSinceEpoch(long millisFromEpoch)
        {
            DateTime epoch = Epoch;
            return epoch.AddMilliseconds(millisFromEpoch);
        }

        private static List<TopicPartition> GetTopicPartitions()
        {
            var tp = new List<TopicPartition>();
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "b-6.socmsk-nxt.dztv1a.c4.kafka.eu-west-1.amazonaws.com:9092" }).Build())
            {
                var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(20));
                meta.Topics.ForEach(topic => {
                    if (topic.Topic == "sbbme_agglomerated_events")
                    {
                        foreach (PartitionMetadata partition in topic.Partitions)
                        {
                            tp.Add(new TopicPartition(topic.Topic, partition.PartitionId));
                        }
                    }
                });
            }
            return tp;
        }
    }
}