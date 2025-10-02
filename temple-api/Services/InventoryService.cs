using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using TempleApi.Enums;

namespace TempleApi.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly TempleDbContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(TempleDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<InventoryDto>> GetAllAsync()
        {
            try
            {
                var inventories = await _context.Inventories
                    .Include(i => i.Temple)
                    .Include(i => i.Area)
                    .Select(i => MapToDto(i))
                    .ToListAsync();

                return inventories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all inventories");
                throw;
            }
        }

        public async Task<IEnumerable<InventoryDto>> GetByTempleIdAsync(int templeId)
        {
            try
            {
                var inventories = await _context.Inventories
                    .Include(i => i.Temple)
                    .Include(i => i.Area)
                    .Where(i => i.TempleId == templeId)
                    .Select(i => MapToDto(i))
                    .ToListAsync();

                return inventories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventories for temple {TempleId}", templeId);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryDto>> GetByAreaIdAsync(int areaId)
        {
            try
            {
                var inventories = await _context.Inventories
                    .Include(i => i.Temple)
                    .Include(i => i.Area)
                    .Where(i => i.AreaId == areaId)
                    .Select(i => MapToDto(i))
                    .ToListAsync();

                return inventories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventories for area {AreaId}", areaId);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryDto>> GetByItemWorthAsync(ItemWorth itemWorth)
        {
            try
            {
                var inventories = await _context.Inventories
                    .Include(i => i.Temple)
                    .Include(i => i.Area)
                    .Where(i => i.ItemWorth == itemWorth)
                    .Select(i => MapToDto(i))
                    .ToListAsync();

                return inventories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventories by item worth {ItemWorth}", itemWorth);
                throw;
            }
        }

        public async Task<InventoryDto?> GetByIdAsync(int id)
        {
            try
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Temple)
                    .Include(i => i.Area)
                    .FirstOrDefaultAsync(i => i.Id == id);

                return inventory != null ? MapToDto(inventory) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory with id {Id}", id);
                throw;
            }
        }

        public async Task<InventoryDto> CreateAsync(CreateInventoryDto dto)
        {
            try
            {
                // Validate Temple exists
                var temple = await _context.Temples.FindAsync(dto.TempleId);
                if (temple == null)
                {
                    throw new InvalidOperationException($"Temple with id {dto.TempleId} not found");
                }

                // Validate Area exists
                var area = await _context.Areas.FindAsync(dto.AreaId);
                if (area == null)
                {
                    throw new InvalidOperationException($"Area with id {dto.AreaId} not found");
                }

                // Validate Area belongs to Temple
                if (area.TempleId != dto.TempleId)
                {
                    throw new InvalidOperationException($"Area {dto.AreaId} does not belong to Temple {dto.TempleId}");
                }

                var inventory = new Inventory
                {
                    TempleId = dto.TempleId,
                    AreaId = dto.AreaId,
                    ItemName = dto.ItemName,
                    ItemWorth = dto.ItemWorth,
                    ApproximatePrice = dto.ApproximatePrice,
                    Quantity = dto.Quantity,
                    CreatedDate = dto.CreatedDate,
                    Active = dto.Active,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var created = await _context.Inventories
                    .Include(i => i.Temple)
                    .Include(i => i.Area)
                    .FirstAsync(i => i.Id == inventory.Id);

                _logger.LogInformation("Created inventory item {ItemName} with id {Id}", inventory.ItemName, inventory.Id);
                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory item");
                throw;
            }
        }

        public async Task<InventoryDto?> UpdateAsync(int id, CreateInventoryDto dto)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null)
                {
                    return null;
                }

                // Validate Temple exists if changing
                if (dto.TempleId != inventory.TempleId)
                {
                    var temple = await _context.Temples.FindAsync(dto.TempleId);
                    if (temple == null)
                    {
                        throw new InvalidOperationException($"Temple with id {dto.TempleId} not found");
                    }
                }

                // Validate Area exists if changing
                if (dto.AreaId != inventory.AreaId)
                {
                    var area = await _context.Areas.FindAsync(dto.AreaId);
                    if (area == null)
                    {
                        throw new InvalidOperationException($"Area with id {dto.AreaId} not found");
                    }

                    // Validate Area belongs to Temple
                    if (area.TempleId != dto.TempleId)
                    {
                        throw new InvalidOperationException($"Area {dto.AreaId} does not belong to Temple {dto.TempleId}");
                    }
                }

                inventory.TempleId = dto.TempleId;
                inventory.AreaId = dto.AreaId;
                inventory.ItemName = dto.ItemName;
                inventory.ItemWorth = dto.ItemWorth;
                inventory.ApproximatePrice = dto.ApproximatePrice;
                inventory.Quantity = dto.Quantity;
                inventory.CreatedDate = dto.CreatedDate;
                inventory.Active = dto.Active;
                inventory.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var updated = await _context.Inventories
                    .Include(i => i.Temple)
                    .Include(i => i.Area)
                    .FirstAsync(i => i.Id == inventory.Id);

                _logger.LogInformation("Updated inventory item {ItemName} with id {Id}", inventory.ItemName, inventory.Id);
                return MapToDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory with id {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null)
                {
                    return false;
                }

                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted inventory item with id {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory with id {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryDto>> GetActiveItemsAsync()
        {
            try
            {
                var inventories = await _context.Inventories
                    .Include(i => i.Temple)
                    .Include(i => i.Area)
                    .Where(i => i.Active && i.IsActive)
                    .Select(i => MapToDto(i))
                    .ToListAsync();

                return inventories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active inventories");
                throw;
            }
        }

        public async Task<decimal> GetTotalValueByAreaAsync(int areaId)
        {
            try
            {
                var totalValue = await _context.Inventories
                    .Where(i => i.AreaId == areaId && i.Active && i.IsActive)
                    .SumAsync(i => i.ApproximatePrice * i.Quantity);

                return totalValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total value for area {AreaId}", areaId);
                throw;
            }
        }

        public async Task<int> GetTotalQuantityByTempleAsync(int templeId)
        {
            try
            {
                var totalQuantity = await _context.Inventories
                    .Where(i => i.TempleId == templeId && i.Active && i.IsActive)
                    .SumAsync(i => i.Quantity);

                return totalQuantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total quantity for temple {TempleId}", templeId);
                throw;
            }
        }

        private static InventoryDto MapToDto(Inventory inventory)
        {
            return new InventoryDto
            {
                Id = inventory.Id,
                TempleId = inventory.TempleId,
                TempleName = inventory.Temple?.Name,
                AreaId = inventory.AreaId,
                AreaName = inventory.Area?.Name,
                ItemName = inventory.ItemName,
                ItemWorth = inventory.ItemWorth,
                ApproximatePrice = inventory.ApproximatePrice,
                Quantity = inventory.Quantity,
                CreatedDate = inventory.CreatedDate,
                Active = inventory.Active,
                CreatedAt = inventory.CreatedAt,
                UpdatedAt = inventory.UpdatedAt,
                IsActive = inventory.IsActive
            };
        }
    }
}
