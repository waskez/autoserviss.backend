using Newtonsoft.Json;

namespace AutoServiss.Database
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; }
    }
}
