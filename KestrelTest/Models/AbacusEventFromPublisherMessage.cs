using System;
using System.Collections.Generic;
using System.Linq;
using AbacusViewer.Contracts.V2;
using Lightyear.Common.Agglomerator.Contracts.Proto.PesV3;
using Newtonsoft.Json;

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

        public IEnumerable<AbacusSelection> Selections
        {
            get
            {
                var ret = new List<AbacusSelection>();
                if (_agglomeratedEvent?.Markets != null)
                {
                    foreach (var m in _agglomeratedEvent.Markets.AsParallel())
                    {
                        foreach (var s in m.Selections)
                        {
                            ret.Add(new AbacusSelection
                            {
                                MarketTypeId = m.MarketTypeId,
                                SelectionId = s.SelectionId,
                                SelectionIdentifier = $"{s.SelectionId}", //"N/A", //s.SelectionIdentifier, // TODO - add this assignment when the AgglomeratedSelection has this property
                                Probability = s.AgglomeratedOutcomes.Count(x => x.Equals(1)) / (double)s.AgglomeratedOutcomes.Count(),
                                Outcomes = s.AgglomeratedOutcomes.ToByteArray()
                            });
                        }
                    }
                }
                return ret;
            }
        }
    }
}