using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServiss.Database
{
    public class Uznemums : BaseEntity
    {
        [Required(ErrorMessage = "Nav norādīts Nosaukums")]
        public string Nosaukums { get; set; }

        [Required(ErrorMessage = "Nav norādīts Reģistrācijas numurs")]
        public string RegNumurs { get; set; }

        [Required(ErrorMessage = "Nav norādīts PVN maksātāja numurs")]
        public string PvnNumurs { get; set; }

        [Required(ErrorMessage = "Nav norādīta E-pasta adrese")]
        public string Epasts { get; set; }

        [Required(ErrorMessage = "Nav norādīts Tālruņa numurs")]
        public string Talrunis { get; set; }
        public string Piezimes { get; set; }

        public List<UznemumaAdrese> Adreses { get; set; }

        public List<UznemumaBanka> Bankas { get; set; }

        public List<UznemumaDarbinieks> UznemumaDarbinieki { get; set; }

        [JsonIgnore]
        public List<ServisaLapa> ServisaLapas { get; set; }

        [NotMapped]
        public List<Darbinieks> Darbinieki { get; set; } // priekš frontend
    }
}
