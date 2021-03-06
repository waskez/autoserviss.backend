﻿using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public class PaveiktaisDarbs
    {
        public int Id { get; set; }
        public string Nosaukums { get; set; }
        public int Skaits { get; set; }
        public string Mervieniba { get; set; }
        public decimal Cena { get; set; }

        [JsonIgnore]
        public int ServisaLapasId { get; set; }

        [JsonIgnore]
        public ServisaLapa ServisaLapa { get; set; }
    }
}
