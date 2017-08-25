using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AutoServiss.Database
{
    public class Adrese
    {
        public int Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AdresesVeids Veids { get; set; }
        public string Nosaukums { get; set; }

        [JsonIgnore]
        public int KlientaId { get; set; }
        [JsonIgnore]
        public Klients Klients { get; set; }
    }
}
