using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoServiss.Database
{
    public class Klients : BaseEntity
    {
        public KlientaVeids Veids { get; set; }

        public string Nosaukums { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RegNumurs { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PvnNumurs { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Kontaktpersona { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Epasts { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Talrunis { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Piezimes { get; set; }

        public List<KlientaAdrese> Adreses { get; set; }

        public List<KlientaBanka> Bankas { get; set; }

        public List<Transportlidzeklis> Transportlidzekli { get; set; }

        [JsonIgnore]
        public List<ServisaLapa> ServisaLapas { get; set; }

        public bool ShouldSerializeTransportlidzekli()
        {
            return Transportlidzekli == null ? false : Transportlidzekli.Count > 0;
        }
    }
}
