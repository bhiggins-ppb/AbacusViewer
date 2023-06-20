using Google.Protobuf;
using Lightyear.Common.Agglomerator.Contracts.Proto.PesV3;
using Newtonsoft.Json;
using System.Text;

namespace AbacusViewer.Models
{
    public class AbacusEventFromPublisherMessage
    {
        private readonly PublisherMessage _agglomeratedEvent;

        public AbacusEventFromPublisherMessage(PublisherMessage agglomeratedEvent)
        {
            _agglomeratedEvent = agglomeratedEvent;
        }

        [JsonProperty("received_time")]
        public DateTime ReceivedTime { get; set; }

        [JsonProperty("received_time_pretty")]
        public string ReceivedTimePretty { get { return ReceivedTime.ToString("dd/MM/yyyy HH:mm:ss"); } }

        [JsonProperty("event_id")]
        public long? EventId
        {
            get { return _agglomeratedEvent.EventId; }
            set { _agglomeratedEvent.EventId = (long)value; }
        }

        [JsonProperty("subclass_id")]
        public long SubClassId { get { return _agglomeratedEvent.SubclassId; } }

        [JsonProperty("event_type_id")]
        public long EventTypeId { get { return _agglomeratedEvent.EventTypeId; } }

        [JsonProperty("market_count")]
        public int MarketCount { get { return _agglomeratedEvent.Markets.Count(); } }

        public IEnumerable<AbacusSelection> GetSelections(Dictionary<long, string> emsSelectionLookup)
        {
            var ret = new List<AbacusSelection>();
            if (_agglomeratedEvent?.Markets != null)
            {
                foreach (var m in _agglomeratedEvent.Markets.AsParallel())
                {
                    foreach (var s in m.Selections)
                    {
                        var selectionVector = UnpackBits(s.AgglomeratedOutcomes, 10000);

                        var outcomes = new StringBuilder();
                        foreach(var bit in selectionVector)
                        {
                            outcomes.Append(bit);
                        }

                        ret.Add(new AbacusSelection
                        {
                            MarketTypeId = m.MarketTypeId,
                            SelectionId = emsSelectionLookup.ContainsKey(s.SelectionId) ? emsSelectionLookup[s.SelectionId] : $"{s.SelectionId}",
                            Probability = selectionVector.Count(x => x.Equals(1)) / (double)selectionVector.Count(),
                            Outcomes = selectionVector,
                            UnpackedOutcomes = outcomes.ToString()
                        });
                           
                    }
                }
            }
            return ret;
        }

        private static byte[] UnpackBits(ByteString byteString, int numSimulations)
        {
            var values = new byte[numSimulations];
            for (var i = 0; i < numSimulations; ++i)
            {
                var wordIndex = i >> 3;
                var bitIndex = i & 0x7;
                var word = byteString[wordIndex];
                values[i] = (byte)((word >> bitIndex) & 1);
            }
            return values;
        }
    }
}