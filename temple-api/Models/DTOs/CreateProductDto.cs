namespace TempleApi.Models.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Keep for backward compatibility
        public int? CategoryId { get; set; } // New field for Category relationship
        public int Quantity { get; set; } = 0;
        public int StockQuantity { get; set; } = 0;
        public int MinStockLevel { get; set; } = 0;
        public decimal Price { get; set; }
        public string Status { get; set; } = "Active";
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
