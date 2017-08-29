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

        public int TransportlidzeklaId { get; set; }
        public string TransportlidzeklaNumurs { get; set; }
        public string TransportlidzeklaMarka { get; set; }
        public string TransportlidzeklaModelis { get; set; }
        public int TransportlidzeklaGads { get; set; }

        public int KlientaId { get; set; }
        public KlientaVeids KlientaVeids { get; set; }
        public string KlientaNosaukums { get; set; }
        public string KlientaRegNumurs { get; set; }
        public string KlientaPvnNumurs { get; set; }
        [JsonIgnore]
        public string KlientaAdreses { get; set; } // JSON
        [NotMapped]
        public List<Adrese> Adreses { get; set; }
        [JsonIgnore]
        public string KlientaBankas { get; set; } // JSON
        [NotMapped]
        public List<Banka> Bankas { get; set; }
        [JsonIgnore]
        public string KlientaKontakti { get; set; } // JSON  
        [NotMapped]
        public Kontakti Kontakti { get; set; }

        public List<Defekts> Defekti { get; set; }
        public List<RezervesDala> RezervesDalas { get; set; }
        public List<PaveiktaisDarbs> PaveiktieDarbi { get; set; }
        public List<Mehanikis> Mehaniki { get; set; }

        public decimal KopejaSumma { get; set; }
    }
}