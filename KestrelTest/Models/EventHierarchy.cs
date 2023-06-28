namespace AbacusViewer.Models
{
    public class EventHierarchy
    {
        public long PaddyPowerId { get; set; }
        public int EventTypeId { get; set; }
        public string EventTypeName { get; set; }
        public string Name { get; set; }
        public string Sort { get; set; }
        public int DisplayOrder { get; set; }
        public IEnumerable<Market> Markets { get; set; }
    }
}