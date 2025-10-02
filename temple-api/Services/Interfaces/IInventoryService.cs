using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TempleApi.Models.DTOs;
using TempleApi.Enums;

namespace TempleApi.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetAllAsync();
        Task<IEnumerable<InventoryDto>> GetByTempleIdAsync(int templeId);
        Task<IEnumerable<InventoryDto>> GetByAreaIdAsync(int areaId);
        Task<IEnumerable<InventoryDto>> GetByItemWorthAsync(ItemWorth itemWorth);
        Task<InventoryDto?> GetByIdAsync(int id);
        Task<InventoryDto> CreateAsync(CreateInventoryDto dto);
        Task<InventoryDto?> UpdateAsync(int id, CreateInventoryDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<InventoryDto>> GetActiveItemsAsync();
        Task<decimal> GetTotalValueByAreaAsync(int areaId);
        Task<int> GetTotalQuantityByTempleAsync(int templeId);
    }
}
