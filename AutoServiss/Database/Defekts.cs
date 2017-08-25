using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AutoServiss.Database
{
    public class Defekts
    {
        public int Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DefektaVeids Veids { get; set; }
        public string Nosaukums { get; set; }

        public int ServisaLapasId { get; set; }

        [JsonIgnore]
        public ServisaLapa ServisaLapa { get; set; }
    }
}
