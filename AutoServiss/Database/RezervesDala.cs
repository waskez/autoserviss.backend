using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public class RezervesDala
    {
        public int Id { get; set; }
        public string Nosaukums { get; set; }
        public decimal Skaits { get; set; }
        public string Mervieniba { get; set; }
        public decimal Cena { get; set; }

        public int ServisaLapasId { get; set; }

        [JsonIgnore]
        public ServisaLapa ServisaLapa { get; set; }
    }
}
