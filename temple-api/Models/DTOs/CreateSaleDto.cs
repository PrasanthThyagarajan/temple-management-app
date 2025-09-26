namespace TempleApi.Models.DTOs
{
    public class CreateSaleDto
    {
        public int UserId { get; set; } // Customer
        public int StaffId { get; set; } // Staff who processes the sale
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Completed";
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int? EventId { get; set; }
        public List<CreateSaleItemDto> SaleItems { get; set; } = new List<CreateSaleItemDto>();
    }

    public class CreateSaleItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
