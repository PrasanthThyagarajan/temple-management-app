namespace TempleApi.Domain.Entities
{
    public class Pooja : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Navigation properties
        public virtual ICollection<PoojaBooking> Bookings { get; set; } = new List<PoojaBooking>();
    }
}
