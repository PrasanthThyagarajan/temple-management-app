using System;
using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class Contribution : BaseEntity
    {
        [Required]
        public int EventId { get; set; }

        public Event? Event { get; set; }

        [Required]
        public int DevoteeId { get; set; }

        public Devotee? Devotee { get; set; }

        [Required]
        public int ContributionSettingId { get; set; }

        public ContributionSetting? ContributionSetting { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime ContributionDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Online, etc.

        [MaxLength(100)]
        public string? TransactionReference { get; set; }
    }
}
