using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoServiss.Database
{
    public class Marka
    {
        public int Id { get; set; }
        public string Nosaukums { get; set; }

        [JsonIgnore]
        public List<Modelis> Modeli { get; set; }
    }
}
