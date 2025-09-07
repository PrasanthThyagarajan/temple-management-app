using TempleApi.Enums;

namespace TempleApi.Models.DTOs
{
    public class CreatePoojaBookingDto
    {
        public int UserId { get; set; } // Customer
        public int PoojaId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int? StaffId { get; set; } // Assigned priest/staff (optional)
    }
}
