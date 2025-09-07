using TempleApi.Enums;

namespace TempleApi.Models.DTOs
{
    public class PoojaBookingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PoojaId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int? StaffId { get; set; }
        public decimal Amount { get; set; }
        public BookingStatus Status { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PoojaName { get; set; } = string.Empty;
        public string? StaffName { get; set; }
    }
}
