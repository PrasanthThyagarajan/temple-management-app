using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRepository<SaleItem> _saleItemRepository;
        private readonly IProductService _productService;

        public SaleService(
            ISaleRepository saleRepository,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IRepository<SaleItem> saleItemRepository,
            IProductService productService)
        {
            _saleRepository = saleRepository;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _saleItemRepository = saleItemRepository;
            _productService = productService;
        }

        public async Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto)
        {
            // Validate that customer and staff exist
            var customer = await _userRepository.GetByIdAsync(createSaleDto.UserId);
            var staff = await _userRepository.GetByIdAsync(createSaleDto.StaffId);
            
            if (customer == null || !customer.IsActive)
            {
                throw new KeyNotFoundException("Customer not found.");
            }
            
            if (staff == null || !staff.IsActive)
            {
                throw new KeyNotFoundException("Staff member not found.");
            }

            // Validate products and quantities
            foreach (var item in createSaleDto.SaleItems)
            {
                var isAvailable = await _productService.CheckProductAvailabilityAsync(item.ProductId, item.Quantity);
                if (!isAvailable)
                {
                    throw new InvalidOperationException($"Product {item.ProductId} is not available in the requested quantity.");
                }
            }

            // Create sale
            var sale = new Sale
            {
                UserId = createSaleDto.UserId,
                StaffId = createSaleDto.StaffId,
                SaleDate = createSaleDto.SaleDate,
                TotalAmount = createSaleDto.TotalAmount,
                DiscountAmount = createSaleDto.DiscountAmount,
                FinalAmount = createSaleDto.FinalAmount,
                PaymentMethod = createSaleDto.PaymentMethod,
                IsActive = createSaleDto.Status == "Completed",
                Notes = createSaleDto.Notes,
                EventId = createSaleDto.EventId,
            };

            await _saleRepository.AddAsync(sale);

            // Create sale items and calculate total
            decimal totalAmount = 0;
            foreach (var itemDto in createSaleDto.SaleItems)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null) continue;

                var saleItem = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = product.Price * itemDto.Quantity
                };

                totalAmount += saleItem.Subtotal;
                await _saleItemRepository.AddAsync(saleItem);

                // Update product quantity
                product.Quantity -= itemDto.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            // Update sale total
            sale.TotalAmount = totalAmount;
            await _saleRepository.UpdateAsync(sale);

            return await GetSaleByIdAsync(sale.Id) ?? throw new InvalidOperationException("Failed to retrieve created sale.");
        }

        public async Task<SaleDto?> GetSaleByIdAsync(int id)
        {
            var sale = await _saleRepository.GetWithDetailsAsync(id);

            return sale != null ? MapToDto(sale) : null;
        }

        public async Task<IEnumerable<SaleDto>> GetAllSalesAsync()
        {
            var sales = await _saleRepository.GetAllWithDetailsAsync();

            return sales.Select(MapToDto);
        }

        public async Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId)
        {
            var sales = await _saleRepository.GetByCustomerAsync(customerId);

            return sales.Select(MapToDto);
        }

        public async Task<IEnumerable<SaleDto>> GetSalesByStaffAsync(int staffId)
        {
            var sales = await _saleRepository.GetByStaffAsync(staffId);

            return sales.Select(MapToDto);
        }

        public async Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _saleRepository.GetByDateRangeAsync(startDate, endDate);

            return sales.Select(MapToDto);
        }

        public async Task<IEnumerable<SaleDto>> SearchSalesAsync(string searchTerm)
        {
            var sales = await _saleRepository.GetAllWithDetailsAsync();
            var filteredSales = sales.Where(s => 
                s.Customer?.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                s.Staff?.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true);
            
            return filteredSales.Select(MapToDto);
        }

        public async Task<SaleDto> UpdateSaleAsync(int id, CreateSaleDto updateSaleDto)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null)
            {
                throw new KeyNotFoundException("Sale not found.");
            }

            // Update sale properties
            sale.UserId = updateSaleDto.UserId;
            sale.StaffId = updateSaleDto.StaffId;
            sale.SaleDate = updateSaleDto.SaleDate;
            sale.TotalAmount = updateSaleDto.TotalAmount;
            sale.DiscountAmount = updateSaleDto.DiscountAmount;
            sale.FinalAmount = updateSaleDto.FinalAmount;
            sale.PaymentMethod = updateSaleDto.PaymentMethod;
            sale.IsActive = updateSaleDto.Status == "Completed";
            sale.Notes = updateSaleDto.Notes;

            await _saleRepository.UpdateAsync(sale);
            return await GetSaleByIdAsync(sale.Id) ?? throw new InvalidOperationException("Failed to retrieve updated sale.");
        }

        public async Task<bool> UpdateSaleStatusAsync(int id, string status)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null)
            {
                return false;
            }

            // Map status string to IsActive boolean
            sale.IsActive = status == "Completed";
            await _saleRepository.UpdateAsync(sale);
            return true;
        }

        public async Task<bool> DeleteSaleAsync(int id)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null)
            {
                return false;
            }

            // Restore product quantities
            var saleItems = await _saleItemRepository.FindAsync(si => si.SaleId == id);

            foreach (var item in saleItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            return await _saleRepository.SoftDeleteAsync(id);
        }

        private static SaleDto MapToDto(Sale sale)
        {
            return new SaleDto
            {
                Id = sale.Id,
                UserId = sale.UserId,
                StaffId = sale.StaffId,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                DiscountAmount = sale.DiscountAmount,
                FinalAmount = sale.FinalAmount,
                PaymentMethod = sale.PaymentMethod,
                Status = sale.IsActive ? "Completed" : "Pending",
                CustomerName = sale.Customer?.FullName ?? string.Empty,
                CustomerPhone = sale.Customer?.Email ?? string.Empty, // Using email as contact
                StaffName = sale.Staff?.FullName ?? string.Empty,
                Notes = sale.Notes,
                SaleItems = sale.SaleItems.Select(si => new SaleItemDto
                {
                    Id = si.Id,
                    ProductId = si.ProductId,
                    ProductName = si.Product?.Name ?? string.Empty,
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice,
                    Subtotal = si.Subtotal
                }).ToList()
            };
        }
    }
}
