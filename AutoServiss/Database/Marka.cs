using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoServiss.Database
{
    public class Marka
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nav norādīts Nosaukums")]
        public string Nosaukums { get; set; }

        [JsonIgnore]
        public List<Modelis> Modeli { get; set; }
    }
}