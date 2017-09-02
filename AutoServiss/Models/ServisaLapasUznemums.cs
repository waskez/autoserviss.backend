using AutoServiss.Database;
using System.Collections.Generic;

namespace AutoServiss.Models
{
    public class ServisaLapasUznemums
    {
        public int Id { get; set; }
        public string Nosaukums { get; set; }
        public List<Mehanikis> Mehaniki { get; set; }
    }
}
