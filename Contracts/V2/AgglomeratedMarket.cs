using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProtoBuf;

namespace AbacusViewer.Contracts.V2
{
    [ProtoContract]
    public sealed class AgglomeratedMarket
    {
        [ProtoMember(1)]
        public long MarketId { get; set; }

        [ProtoMember(2)]
        public long MarketTypeId { get; set; }

        [ProtoMember(3)]
        public double? HandicapValue { get; set; }

        [ProtoMember(4)]
        public ICollection<Contracts.V2.AgglomeratedSelection> Selections { get; set; }
    }
}
