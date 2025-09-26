using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TempleApi.Domain.Entities
{
    public class Area : BaseEntity
    {
        public int TempleId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public virtual Temple Temple { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}


