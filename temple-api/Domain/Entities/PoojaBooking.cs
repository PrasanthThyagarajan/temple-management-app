using TempleApi.Enums;

namespace TempleApi.Domain.Entities
{
    public class PoojaBooking : BaseEntity
    {
        public int UserId { get; set; } // Customer
        public int PoojaId { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public DateTime ScheduledDate { get; set; }
        public int? StaffId { get; set; } // Assigned priest/staff (nullable)
        public decimal Amount { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Navigation properties
        public virtual User Customer { get; set; } = null!;
        public virtual Pooja Pooja { get; set; } = null!;
        public virtual User? Staff { get; set; }
    }
}
