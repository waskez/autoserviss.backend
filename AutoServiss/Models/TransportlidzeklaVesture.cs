using AutoServiss.Database;
using System;
using System.Collections.Generic;

namespace AutoServiss.Models
{
    public class TransportlidzeklaVesture
    {
        public int Id { get; set; }
        public DateTime Datums { get; set; }
        public List<Darbs> PaveiktieDarbi { get; set; }
        public List<RezervesDala> RezervesDalas { get; set; }
    }
}
