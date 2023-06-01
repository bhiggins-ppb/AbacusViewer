using System.Collections.Generic;
using ProtoBuf;

namespace AbacusViewer.Contracts.V2
{
    [ProtoContract]
    public class AgglomeratedEvent
    {
        public AgglomeratedEvent()
        { }

        [ProtoMember(1)]
        public long EventId { get; set; }
        [ProtoMember(2)]
        public long SubclassId { get; set; }
        [ProtoMember(3)]
        public long EventTypeId { get; set; }
        [ProtoMember(4)]
        public int NumberOfSimulations { get; set; }
        [ProtoMember(5)]
        public ICollection<Contracts.V2.AgglomeratedMarket> Markets { get; set; }
    }
}