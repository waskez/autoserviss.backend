﻿using AutoServiss.Database;
using System;
using System.Collections.Generic;

namespace AutoServiss.Models
{
    public class TransportlidzeklaVesture
    {
        public int Id { get; set; }
        public DateTime No { get; set; }
        public DateTime? Lidz { get; set; }
        public List<PaveiktaisDarbs> PaveiktieDarbi { get; set; }
        public List<RezervesDala> RezervesDalas { get; set; }
    }
}
