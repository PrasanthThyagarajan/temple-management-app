using TempleApi.Enums;

namespace TempleApi.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string PasswordHash { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Sale> CustomerSales { get; set; } = new List<Sale>();
        public virtual ICollection<Sale> StaffSales { get; set; } = new List<Sale>();
        public virtual ICollection<PoojaBooking> CustomerBookings { get; set; } = new List<PoojaBooking>();
        public virtual ICollection<PoojaBooking> StaffBookings { get; set; } = new List<PoojaBooking>();
    }
}
