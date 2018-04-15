using System;

namespace AutoServiss.Models
{
    public class RemontuVesture
    {
        public DateTime Datums { get; set; }
        public int KlientaId { get; set; }
        public string Klients { get; set; }
        public int TransportlidzeklaId { get; set; }
        public string Marka { get; set; }
        public string Numurs { get; set; }
        public int ServisaLapasId { get; set; }
    }
}