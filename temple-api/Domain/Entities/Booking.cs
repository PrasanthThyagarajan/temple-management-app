namespace TempleApi.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public int UserId { get; set; }
        public int StaffId { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public decimal? EstimatedAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Notes { get; set; } = string.Empty;
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    }
}


