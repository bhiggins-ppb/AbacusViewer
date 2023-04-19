using System.Collections.Concurrent;
using System.Linq;
using AbacusViewer.Models;
using System;
using AbacusViewer.Contracts.V2;
//using BTS.Infrastructure.DotNetExtensions.Diagnostics;

namespace AbacusViewer.Services
{
    public class EventCollector
    {
        public ConcurrentDictionary<long, AbacusEvent> AbacusEvents { get; }
        private int MaxQueueDepth { get; }
        //private readonly EventConsumer _eventConsumer;

        public EventCollector(//EventConsumer eventConsumer,
                              int maxQueueDepth)
        {
            //_eventConsumer = ArgCheck.IsNotNull(eventConsumer, nameof(eventConsumer));
            MaxQueueDepth = maxQueueDepth;
            AbacusEvents = new ConcurrentDictionary<long, AbacusEvent>();
        }

        public void Start()
        {
            //_eventConsumer.EventReceived += EventReceived;
            //_eventConsumer.StartConsuming();
        }

        private void EventReceived(AgglomeratedEvent agglomeratedEvent)
        {
            var abacusEvent = new AbacusEvent(agglomeratedEvent) { ReceivedTime = DateTime.Now };

            //MvcApplication.Logger.InfoFormat("EventCollector received AbacusEvent with eventId : " + agglomeratedEvent.EventId);

            if (!AbacusEvents.ContainsKey(agglomeratedEvent.EventId))
            {
                if (AbacusEvents.Count == MaxQueueDepth)
                {
                    var oldest = AbacusEvents.OrderBy(x => x.Value.ReceivedTime).First();
                    AbacusEvents.TryRemove(oldest.Key, out AbacusEvent val);

                    //MvcApplication.Logger.InfoFormat("EventCollector dropped oldest event with eventId : " + oldest.Key);
                }

                AbacusEvents.TryAdd(agglomeratedEvent.EventId, abacusEvent);
            }
            else
            {
                AbacusEvents[agglomeratedEvent.EventId] = abacusEvent;
            }
        }
    }
}