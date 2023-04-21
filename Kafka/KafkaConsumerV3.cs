using Confluent.Kafka;
using Google.Protobuf;
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
using static Confluent.Kafka.ConfigPropertyNames;

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

    public static void Main(string[] args)
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

        using var consumer = new ConsumerBuilder<Ignore, byte[]>(new ConsumerConfig()
        {
            BootstrapServers = "b-6.socmsk-nxt.dztv1a.c4.kafka.eu-west-1.amazonaws.com:9092", //kafkaConsumerConfiguration.BrokerList,
            GroupId = "test-CG123", //kafkaConsumerConfiguration.ConsumerGroup,

            AutoOffsetReset = AutoOffsetReset.Earliest,
            AutoCommitIntervalMs = 10_000,
        }).Build();

        
        consumer.Assign(topic_partitions);

        List<TopicPartitionTimestamp> new_times = new List<TopicPartitionTimestamp>();
        foreach (TopicPartition tp in topic_partitions)
        {
            new_times.Add(new TopicPartitionTimestamp(tp, new Timestamp(DateTime.Now.AddMinutes(-30))));
        }

        List<TopicPartitionOffset> seeked_offsets = consumer.OffsetsForTimes(new_times, TimeSpan.FromSeconds(40));
        string s = "";
        foreach (TopicPartitionOffset tpo in seeked_offsets)
        {
            s += $"{tpo.TopicPartition}: {tpo.Offset.Value}\n";
        }
        Console.WriteLine(s);
        consumer.Close();

        using _consumer = new KafkaConsumer();

        ConsumeLoopInternal()
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