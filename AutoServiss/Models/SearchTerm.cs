using System.ComponentModel.DataAnnotations;

namespace AutoServiss.Models
{
    public class SearchTerm
    {
        [Required(ErrorMessage = "Nav norādīta \"Value\" vērtība")]
        public string Value { get; set; }
        public int Id { get; set; }
    }
}
