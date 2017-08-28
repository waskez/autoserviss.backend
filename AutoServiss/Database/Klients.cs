using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoServiss.Database
{
    public class Klients
    {
        public int Id { get; set; }

        public KlientaVeids Veids { get; set; }

        public string Nosaukums { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RegNumurs { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PvnNumurs { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Epasts { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Talrunis { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Piezimes { get; set; }

        public List<Adrese> Adreses { get; set; }

        public List<Banka> Bankas { get; set; }

        public List<Transportlidzeklis> Transportlidzekli { get; set; }

        //public bool ShouldSerializeAdreses()
        //{
        //    return Adreses == null ? false : Adreses.Count > 0;
        //}

        //public bool ShouldSerializeBankas()
        //{
        //    return Bankas == null ? false : Bankas.Count > 0;
        //}

        public bool ShouldSerializeTransportlidzekli()
        {
            return Transportlidzekli == null ? false : Transportlidzekli.Count > 0;
        }
    }
}
