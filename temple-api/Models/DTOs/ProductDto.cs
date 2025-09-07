namespace TempleApi.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Keep for backward compatibility
        public int? CategoryId { get; set; } // New field for Category relationship
        public CategoryDto? CategoryNavigation { get; set; } // Navigation property
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockLevel { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Active";
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
