using System;

namespace TempleApi.Models.DTOs
{
    public class ContributionDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public int DevoteeId { get; set; }
        public string? DevoteeName { get; set; }
        public int ContributionSettingId { get; set; }
        public string? ContributionSettingName { get; set; }
        public decimal Amount { get; set; }
        public DateTime ContributionDate { get; set; }
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? TransactionReference { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateContributionDto
    {
        public int EventId { get; set; }
        public int DevoteeId { get; set; }
        public int ContributionSettingId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ContributionDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? TransactionReference { get; set; }
    }

    public class ContributionSummaryDto
    {
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public decimal TotalAmount { get; set; }
        public int ContributionCount { get; set; }
        public DateTime LastContributionDate { get; set; }
    }
}
