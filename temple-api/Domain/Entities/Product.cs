namespace TempleApi.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Keep for backward compatibility
        public int? CategoryId { get; set; } // New foreign key
        public int Quantity { get; set; } = 0;
        public int MinStockLevel { get; set; } = 0;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Category? CategoryNavigation { get; set; }
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
