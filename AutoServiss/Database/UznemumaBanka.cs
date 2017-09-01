using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public class UznemumaBanka
    {
        public int Id { get; set; }
        public string Nosaukums { get; set; }
        public string Kods { get; set; }
        public string Konts { get; set; }

        [JsonIgnore]
        public int UznemumaId { get; set; }

        [JsonIgnore]
        public Uznemums Uznemums { get; set; }
    }
}
