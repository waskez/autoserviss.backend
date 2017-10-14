using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoServiss.Database
{
    public class Transportlidzeklis : BaseEntity
    {
        [Required(ErrorMessage = "Nav norādīts Reģistrācija numurs")]
        public string Numurs { get; set; }

        [Required(ErrorMessage = "Nav norādīta Marka")]
        public string Marka { get; set; }

        [Required(ErrorMessage = "Nav norādīts Modelis")]
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

        [Range(1, int.MaxValue, ErrorMessage = "Nav norādīts KlientaId")]
        public int KlientaId { get; set; }

        [JsonIgnore]
        public Klients Klients { get; set; }

        [JsonIgnore]
        public List<ServisaLapa> ServisaLapas { get; set; }
    }
}
