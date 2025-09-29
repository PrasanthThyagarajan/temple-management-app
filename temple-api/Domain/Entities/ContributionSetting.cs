using System;
using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class ContributionSetting : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int EventId { get; set; }

        public Event? Event { get; set; }

        [Required]
        [MaxLength(20)]
        public string ContributionType { get; set; } = "Single"; // Single or Recurring

        [Required]
        public decimal Amount { get; set; }

        // For recurring contributions
        public int? RecurringDay { get; set; } // Day of month (1-31) for monthly recurring

        [MaxLength(20)]
        public string? RecurringFrequency { get; set; } // Monthly, Weekly, etc.
    }
}
