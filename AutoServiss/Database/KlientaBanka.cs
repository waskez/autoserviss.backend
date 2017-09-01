using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public class KlientaBanka
    {
        public int Id { get; set; }
        public string Nosaukums { get; set; }
        public string Kods { get; set; }
        public string Konts { get; set; }

        [JsonIgnore]
        public int KlientaId { get; set; }

        [JsonIgnore]
        public Klients Klients { get; set; }
    }
}
