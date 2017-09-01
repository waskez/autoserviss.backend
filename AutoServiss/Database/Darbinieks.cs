using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServiss.Database
{
    public class Darbinieks
    {
        public int Id { get; set; }
        public string Avatar { get; set; }
        public string PilnsVards { get; set; }
        public string Amats { get; set; }
        public string Epasts { get; set; }
        public string Talrunis { get; set; }
        public string Lietotajvards { get; set; }
        public string Parole { get; set; }
        public bool Administrators { get; set; }
        public bool Aktivs { get; set; }
        public string RefreshToken { get; set; }

        public List<UznemumaDarbinieks> Uznemumi { get; set; }

        [NotMapped]
        public Uznemums Uznemums { get; set; } // priekš frontend
    }
}
