namespace TempleApi.Models.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StaffId { get; set; }
        public int? ProductId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Completed";
        public int? SalesBookingStatusId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();
    }

    public class SaleItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
