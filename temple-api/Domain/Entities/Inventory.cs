using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TempleApi.Enums;

namespace TempleApi.Domain.Entities
{
    public class Inventory : BaseEntity
    {
        public int TempleId { get; set; }
        
        public int AreaId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;
        
        public ItemWorth ItemWorth { get; set; }
        
        public decimal ApproximatePrice { get; set; }
        
        public int Quantity { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public bool Active { get; set; } = true;
        
        // Navigation properties
        [JsonIgnore]
        public virtual Temple Temple { get; set; } = null!;
        
        [JsonIgnore]
        public virtual Area Area { get; set; } = null!;
    }
}
