using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProtoBuf;

namespace AbacusViewer.Contracts.V2
{
    [ProtoContract]
    public sealed class AgglomeratedSelection
    {
        [ProtoMember(1)]
        public long SelectionId { get; set; }

        [ProtoMember(2)]
        public byte[] AgglomeratedOutcomes { get; set; }
    }
}
