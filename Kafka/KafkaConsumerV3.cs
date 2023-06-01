using Confluent.Kafka;
using Google.Protobuf;
using Lightyear.Common.Agglomerator.Contracts.Proto.PesV3;
//using KestrelTest.Core;
//using Lightyear.Common.Agglomerator.Contracts.Proto.PesV3;
//using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Confluent.Kafka.ConfigPropertyNames;

namespace PesToRabbitBridge.Core.Kafka;

public sealed class KafkaConsumer : IDisposable
{
    private static readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager = new();
    private const int MemoryStreamInitialSize = 10 * 1024 * 1024;

    private static HashSet<string> KnownHeaders = new HashSet<string>() {
        "ppCorrelationId",
        "ppEventId",
        "inplay",
        "ppLyExecutorRequestIn",
        "ppLyAgglomeratorRequestIn",
        "ppLyExecutorDuration",
        "ppLyAgglomeratorDuration"
    };

    //private readonly ILogger<KafkaConsumerV3> _logger;
    //private readonly KafkaConsumerConfiguration _kafkaConsumerConfiguration;                     
    private readonly IConsumer<string, byte[]> _consumer;
    private readonly string _topicName;


    //b-6.socmsk-nxt.dztv1a.c4.kafka.eu-west-1.amazonaws.com:9092
    //sbbme_agglomerated_events

    public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static DateTime FromMillisSinceEpoch(long millisFromEpoch)
    {
        DateTime epoch = Epoch;
        return epoch.AddMilliseconds(millisFromEpoch);
    }

    public static void Main(string[] args)
    {
        var message = GetMostRecentMessageForEventId(2849583);
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

    public static void Main2(string[] args)
    {
        //var consumer = new KafkaConsumer();
        using var shutdownCts = new CancellationTokenSource();
        //var consumerTask = consumer.ConsumeLoop(
        //    //(msg, metadata) => {
        //    //    publisher.PublishAsync(msg, metadata).GetAwaiter().GetResult();
        //    //},
        //    shutdownCts.Token
        //);

        List<TopicPartition> topic_partitions = GetTopicPartitions();

        using var consumer = new ConsumerBuilder<Ignore, Ignore>(new ConsumerConfig()
        {
            BootstrapServers = "b-6.socmsk-nxt.dztv1a.c4.kafka.eu-west-1.amazonaws.com:9092", //kafkaConsumerConfiguration.BrokerList,
            GroupId = "test-CG124", //kafkaConsumerConfiguration.ConsumerGroup,

            AutoOffsetReset = AutoOffsetReset.Earliest,
            AutoCommitIntervalMs = 10_000,
        }).Build();

        //consumer.Assign(topic_partitions);
        int desiredEventId = 9797467;
        //consumer.Assign(topic_partitions.Where(tp => tp.Partition.Value == desiredEventId % topic_partitions.Count));

        List<TopicPartitionTimestamp> new_times = new List<TopicPartitionTimestamp>();
        foreach (TopicPartition tp in topic_partitions)
        {
            new_times.Add(new TopicPartitionTimestamp(tp, new Timestamp(DateTime.Now.AddMinutes(-6000))));
        }

        List<TopicPartitionOffset> seeked_offsets = consumer.OffsetsForTimes(new_times, TimeSpan.FromSeconds(40));
        string s = "";
        foreach (TopicPartitionOffset tpo in seeked_offsets)
        {
            s += $"{tpo.TopicPartition}: {tpo.Offset.Value}\n";
        }
        Console.WriteLine(s);

        consumer.Assign(seeked_offsets.Where(o => o.Partition.Value == desiredEventId % topic_partitions.Count));

        //consumer.Seek(seeked_offsets.Single(o => o.TopicPartition.Partition.Value == desiredEventId % topic_partitions.Count));

        var result = consumer.Consume(5000);
        while (result != null)
        {
            string eventId = Encoding.Default.GetString(result.Message.Headers.Single(h => h.Key == "ppEventId").GetValueBytes());
            var publishTime = FromMillisSinceEpoch(long.Parse(Encoding.Default.GetString(result.Message.Headers.Single(h => h.Key == "ppLyKafkaPublishTime").GetValueBytes())));
            Console.WriteLine($"Partition: {result.TopicPartition.Partition.Value}, Event Id: {eventId}, Publish time: {publishTime}");

            result = consumer.Consume(5000);
        }

        consumer.Close();

        //using _consumer = new KafkaConsumer();

        //ConsumeLoopInternal()
        //consumer.ConsumeLoop(shutdownCts.Token);

    }

    public static List<TopicPartition> GetTopicPartitions()
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

    public KafkaConsumer(//ILogger<KafkaConsumerV3> logger,  KafkaConsumerConfiguration kafkaConsumerConfiguration
        )
    {
        //_logger = null;// ArgCheck.IsNotNull(logger, nameof(logger));
        //_kafkaConsumerConfiguration = null;// ArgCheck.IsNotNull(kafkaConsumerConfiguration, nameof(kafkaConsumerConfiguration));
        _topicName = "sbbme_agglomerated_events";//kafkaConsumerConfiguration.TopicName;

        // build the actual producer
        var consumerConfig = new ConsumerConfig()
        {
            BootstrapServers = "b-6.socmsk-nxt.dztv1a.c4.kafka.eu-west-1.amazonaws.com:9092", //kafkaConsumerConfiguration.BrokerList,
            GroupId = "test-CG123", //kafkaConsumerConfiguration.ConsumerGroup,
            
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AutoCommitIntervalMs = 10_000,
        };

       // new WatermarkOffsets(1, 1);//.GetWatermarkOffsets(TopicPartition topicPartition)

        _consumer = new ConsumerBuilder<string, byte[]>(consumerConfig)
            .SetErrorHandler(ErrorHandler)
            .Build();

        _consumer.Subscribe(_topicName);

        //_logger.LogInformation("Created consumer for brokers {list} and topic {topic}",
        //    kafkaConsumerConfiguration.BrokerList,
        //    _topicName
        //);
    }



    private void ErrorHandler(IConsumer<string, byte[]> sender, Error error)
    {
        /*if (error.IsError)
        {
            _logger.LogError("Kafka producer error: {error}", error);
        }
        else
        {
            _logger.LogInformation("Kafka producer info: {info}", error);
        }*/
    }

    private void ConsumeLoopInternal(//Action<PublisherMessage, PublishMetadata> handler, 
        CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Started kafka consumer loop");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume();// TimeSpan.FromMilliseconds(5000));
                if (consumeResult is not null && consumeResult.Message is not null)
                {
                    // decode headers
                    var msg = consumeResult.Message;
                    var msgHeaders = msg.Headers.ToDictionary(
                        h => h.Key,
                        h => Encoding.UTF8.GetString(h.GetValueBytes())
                    );

                    if (msgHeaders["ppEventId"] != "0")
                        Console.WriteLine(msgHeaders["ppEventId"]);
                    if (msgHeaders["ppEventId"] == "32720416")
                    {

                        Console.WriteLine("Here");
                        /*
                        // extract upstream headers (i.e. the ones we don't explicitly deal with)
                        var upstreamHeaders = msgHeaders.Where(kvp => !KnownHeaders.Contains(kvp.Key))
                            .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

                        // construct metadata for publishing
                        var metadata = new PublishMetadata(
                            Guid.Parse(msgHeaders["ppCorrelationId"]),
                            bool.Parse(msgHeaders["inplay"]),
                            long.Parse(msgHeaders["ppLyExecutorDuration"]),
                            long.Parse(msgHeaders["ppLyAgglomeratorDuration"]),
                            DateTimeEx.FromMillisSinceEpoch(long.Parse(msgHeaders["ppLyExecutorRequestIn"])),
                            DateTimeEx.FromMillisSinceEpoch(long.Parse(msgHeaders["ppLyAgglomeratorRequestIn"])),
                            upstreamHeaders
                                            );*/

                        // decode V3 payload
                        using var ms = new MemoryStream(msg.Value);
                        using var gzdec = new GZipStream(ms, CompressionMode.Decompress);

                        //var publisherMessage = PublisherMessage.Parser.ParseFrom(gzdec);

                        //_logger.LogInformation("Deserialized {size} bytes with CorrelationId={correlationId}",
                        //    msg.Value.Length,
                        //    metadata.CorrelationId
                        //);

                        // send to handler
                        //handler(publisherMessage, metadata);

                        //_logger.LogInformation("Completed handling CorrelationId={correlationId}",
                        //    metadata.CorrelationId
                        //);                        
                    }
                }
            }
            catch (Exception exc)
            {
                const int backoffSeconds = 10;
                //_logger.LogError(exc, "Failed to consume message due to an unexpected exception. Backing off and retrying after {backoff}s.", backoffSeconds);
                Thread.Sleep(TimeSpan.FromSeconds(backoffSeconds));
            }
        }

        // shutdown -- explicitly call close on the consumer; it doesn't do this automatically when disposed.
        //_logger.LogInformation("Shutting down {name}...", nameof(KafkaConsumerV3));
        _consumer.Close();
        //_logger.LogInformation("Consumer closed");
    }

    // Call this once!
    // public Task ConsumeLoop(//Action<PublisherMessage, PublishMetadata> handler, 
    public void ConsumeLoop(//Action<PublisherMessage, PublishMetadata> handler, 
        CancellationToken cancellationToken)
    {
        //return Task.Factory.StartNew(
        //    () => ConsumeLoopInternal(//handler,
        //                              cancellationToken),
        //    TaskCreationOptions.LongRunning
        //);
        ConsumeLoopInternal(cancellationToken);
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
}