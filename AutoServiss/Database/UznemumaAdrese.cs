using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AutoServiss.Database
{
    public class UznemumaAdrese
    {
        public int Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AdresesVeids Veids { get; set; }
        public string Nosaukums { get; set; }

        [JsonIgnore]
        public int UznemumaId { get; set; }
        [JsonIgnore]
        public Uznemums Uznemums { get; set; }
    }
}
