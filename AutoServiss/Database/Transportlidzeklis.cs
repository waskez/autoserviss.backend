using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoServiss.Database
{
    public class Transportlidzeklis
    {
        public int Id { get; set; }
        public string Numurs { get; set; }
        public string Marka { get; set; }
        public string Modelis { get; set; }
        public string Krasa { get; set; }
        public int Gads { get; set; }
        public string Vin { get; set; }
        public string Tips { get; set; }
        public string Variants { get; set; }
        public string Versija { get; set; }
        public string Degviela { get; set; }
        public string Tilpums { get; set; }
        public string Jauda { get; set; }
        public string PilnaMasa { get; set; }
        public string Pasmasa { get; set; }
        public string Piezimes { get; set; }
        public int KlientaId { get; set; }

        [JsonIgnore]
        public Klients Klients { get; set; }
    }
}
