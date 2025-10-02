using System;
using TempleApi.Enums;

namespace TempleApi.Models.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int TempleId { get; set; }
        public string? TempleName { get; set; }
        public int AreaId { get; set; }
        public string? AreaName { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public ItemWorth ItemWorth { get; set; }
        public string ItemWorthDisplay => ItemWorth.ToString();
        public decimal ApproximatePrice { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateInventoryDto
    {
        public int TempleId { get; set; }
        public int AreaId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public ItemWorth ItemWorth { get; set; }
        public decimal ApproximatePrice { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool Active { get; set; } = true;
    }
}
