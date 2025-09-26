namespace TempleApi.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public int UserId { get; set; } // Customer
        public int StaffId { get; set; } // Staff who processed the sale
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Notes { get; set; } = string.Empty;
        public int? EventId { get; set; }
        public virtual Event? Event { get; set; }

        // Navigation properties
        public virtual User Customer { get; set; } = null!;
        public virtual User Staff { get; set; } = null!;
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
