using AbacusViewer.Serializers;
using Newtonsoft.Json;

namespace AbacusViewer.Models
{
    public class AbacusSelection
    {
        [JsonProperty("market_type_id")]
        public long MarketTypeId { get; set; }

        [JsonProperty("market_name")]
        public string MarketName {  get { return ((MarketTypeId)Enum.ToObject(typeof(MarketTypeId), MarketTypeId)).ToString(); } }

        [JsonProperty("selection_id")]
        public long SelectionId { get; set; }

        [JsonProperty("probability")]
        public double Probability { get; set; }

        [JsonProperty("commands")]
        public bool Commands { get; set; }

        [JsonProperty("price")]
        public double Price { get { return Probability > 0 ? Math.Round(1.0 / Probability,3) : 0; } }

        public byte[] Outcomes { get; set; }
    }
}