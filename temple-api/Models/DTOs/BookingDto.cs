namespace TempleApi.Models.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StaffId { get; set; }
        public DateTime BookingDate { get; set; }
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public decimal? EstimatedAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Notes { get; set; } = string.Empty;
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string Status { get; set; } = "Pending";

        // Display fields
        public string? CustomerName { get; set; }
        public string? StaffName { get; set; }
        public string? ProductName { get; set; }
        public string? CategoryName { get; set; }
        public string? ApprovedByName { get; set; }
    }

    public class CreateBookingDto
    {
        public int UserId { get; set; }
        public int StaffId { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public decimal? EstimatedAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Notes { get; set; } = string.Empty;
    }
}

