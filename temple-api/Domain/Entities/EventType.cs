using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TempleApi.Domain.Entities
{
    public class EventType : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}


