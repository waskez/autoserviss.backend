using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public class ServisaLapasMehanikis
    {
        public int ServisaLapasId { get; set; }

        [JsonIgnore]
        public ServisaLapa ServisaLapa { get; set; }

        public int MehanikaId { get; set; }

        [JsonIgnore]
        public Darbinieks Mehanikis { get; set; }
    }
}
