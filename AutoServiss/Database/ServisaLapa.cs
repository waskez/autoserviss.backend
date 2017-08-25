using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServiss.Database
{
    public class ServisaLapa
    {
        public int Id { get; set; }

        public DateTime Datums { get; set; }

        public string Piezimes { get; set; }

        public DateTime? Apmaksata { get; set; }

        public int TransportlidzeklaId { get; set; }
        public Transportlidzeklis Transportlidzeklis { get; set; }

        [NotMapped]
        public Klients Klients { get; set; } // transportlīdzekļiem tiek ignorēts klients (JSON), tāpēc ielādējam atsevišķi        

        public List<Defekts> Defekti { get; set; }
        public List<RezervesDala> RezervesDalas { get; set; }
        public List<Darbs> PaveiktieDarbi { get; set; }

        public List<ServisaLapasMehanikis> ServisaLapasMehaniki { get; set; }
    }
}
