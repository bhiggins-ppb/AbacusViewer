using BTS.Infrastructure.NetStandard.Core;

namespace KestrelTest.Core
{
    public class KafkaConsumerConfiguration : Settings<KafkaConsumerConfiguration>
    {
        public string TopicName { get; set; }
        public string ConsumerGroup { get; set; }
        public string BrokerList { get; set; }
    }
}
