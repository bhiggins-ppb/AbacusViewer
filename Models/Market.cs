using System.Collections.Generic;

namespace AbacusViewer.Models
{
    public class Market
    {
        public long PaddyPowerId { get; set; }
        public int MarketTypeId { get; set; }
        public int MarketTypeLinkId { get; set; }
        public string Name { get; set; }
        public bool WillGoBir { get; set; }
        public IList<string> Properties { get; set; }
        public int DisplayOrder { get; set; }
        public double? HandicapLine { get; set; }
        public int? IndexValue { get; set; }
        public IEnumerable<Selection> Selections { get; set; }
        public EachWayTerms EachWayTerms { get; set; }
        public string Availability { get; set; }
    }
}