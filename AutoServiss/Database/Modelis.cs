using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public class Modelis
    {
        public int Id { get; set; }
        public int MarkasId { get; set; }
        public string Nosaukums { get; set; }

        [JsonIgnore]
        public Marka Marka { get; set; }
    }
}
