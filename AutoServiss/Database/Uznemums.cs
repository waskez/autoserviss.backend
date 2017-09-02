using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServiss.Database
{
    public class Uznemums : BaseEntity
    {
        public string Nosaukums { get; set; }
        public string RegNumurs { get; set; }
        public string PvnNumurs { get; set; }
        public string Epasts { get; set; }
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
