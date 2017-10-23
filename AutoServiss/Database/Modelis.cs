using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace AutoServiss.Database
{
    public class Modelis
    {
        public int Id { get; set; }

        [Range(1, Int32.MaxValue, ErrorMessage = "Nav norādīts Markas identifikators")]
        public int MarkasId { get; set; }

        [Required(ErrorMessage = "Nav norādīts Nosaukums")]
        public string Nosaukums { get; set; }

        [JsonIgnore]
        public Marka Marka { get; set; }
    }
}
