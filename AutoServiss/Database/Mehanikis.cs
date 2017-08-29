using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public class Mehanikis
    {
        public int Id { get; set; }
        public string Nosaukums { get; set; }

        [JsonIgnore]
        public int ServisaLapasId { get; set; }

        [JsonIgnore]
        public ServisaLapa ServisaLapa { get; set; }
    }
}
