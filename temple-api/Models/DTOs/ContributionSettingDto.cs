using System;

namespace TempleApi.Models.DTOs
{
    public class ContributionSettingDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public string ContributionType { get; set; } = "Single";
        public decimal Amount { get; set; }
        public int? RecurringDay { get; set; }
        public string? RecurringFrequency { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateContributionSettingDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string ContributionType { get; set; } = "Single";
        public decimal Amount { get; set; }
        public int? RecurringDay { get; set; }
        public string? RecurringFrequency { get; set; }
    }
}
