using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public class UznemumaDarbinieks
    {
        public int UznemumaId { get; set; }

        [JsonIgnore]
        public Uznemums Uznemums { get; set; }

        public int DarbiniekaId { get; set; }

        [JsonIgnore]
        public Darbinieks Darbinieks { get; set; }
    }
}
