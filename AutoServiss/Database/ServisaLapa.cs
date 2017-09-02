using AutoServiss.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

/* Datiem jābūt neatkarīgiem no citām tabulām (gadījumā ja tiek dzēsts Klients, Transportlīdzeklis vai Darbinieks) */
namespace AutoServiss.Database
{
    public class ServisaLapa
    {
        public int Id { get; set; }
        public DateTime Datums { get; set; }
        public string Piezimes { get; set; }
        public DateTime? Apmaksata { get; set; }

        public int UznemumaId { get; set; }
        public Uznemums Uznemums { get; set; }

        public int TransportlidzeklaId { get; set; }
        public Transportlidzeklis Transportlidzeklis { get; set; }

        public int KlientaId { get; set; }
        public Klients Klients { get; set; }

        public List<Defekts> Defekti { get; set; }
        public List<RezervesDala> RezervesDalas { get; set; }
        public List<PaveiktaisDarbs> PaveiktieDarbi { get; set; }
        public List<Mehanikis> Mehaniki { get; set; }

        public decimal KopejaSumma { get; set; }
    }
}