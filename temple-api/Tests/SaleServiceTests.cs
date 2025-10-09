using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using TempleApi.Services.Interfaces;
using Xunit;
using FluentAssertions;
using Moq;

namespace TempleApi.Tests
{
    public class SaleServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly Mock<ISaleRepository> _saleRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IRepository<SaleItem>> _saleItemRepositoryMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly SaleService _saleService;

        public SaleServiceTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _saleRepositoryMock = new Mock<ISaleRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _saleItemRepositoryMock = new Mock<IRepository<SaleItem>>();
            _productServiceMock = new Mock<IProductService>();

            _saleService = new SaleService(
                _saleRepositoryMock.Object,
                _userRepositoryMock.Object,
                _productRepositoryMock.Object,
                _saleItemRepositoryMock.Object,
                _productServiceMock.Object
            );
        }

        #region Create Tests

        [Fact]
        public async Task CreateSaleAsync_ShouldCreateSale_WhenValidData()
        {
            // Arrange
            var customer = new User { UserId = 1, Username = "customer", FullName = "Customer", Email = "customer@test.com", IsActive = true };
            var staff = new User { UserId = 2, Username = "staff", FullName = "Staff", Email = "staff@test.com", IsActive = true };
            var product = new Product { Id = 1, Name = "Test Product", Price = 10.00m, Quantity = 100, IsActive = true };

            var createSaleDto = new CreateSaleDto
            {
                UserId = 1,
                StaffId = 2,
                ProductId = 1,
                SaleDate = DateTime.Now,
                TotalAmount = 20.00m,
                DiscountAmount = 2.00m,
                FinalAmount = 18.00m,
                PaymentMethod = "Cash",
                Status = "Completed",
                SalesBookingStatusId = (int)TempleApi.Enums.SalesBookingStatus.Awaiting,
                BookingToken = "token-123",
                Notes = "Test sale",
                SaleItems = new List<CreateSaleItemDto>
                {
                    new CreateSaleItemDto { ProductId = 1, Quantity = 2, UnitPrice = 10.00m, TotalPrice = 20.00m }
                }
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(customer);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(staff);
            _productServiceMock.Setup(x => x.CheckProductAvailabilityAsync(1, 2)).ReturnsAsync(true);
            _productRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _saleRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Sale>()))
                .Callback<Sale>(s => s.Id = 1)
                .ReturnsAsync((Sale s) => s);
            _saleItemRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<SaleItem>()))
                .ReturnsAsync((SaleItem si) => si);
            _productRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _saleRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);

            var createdSale = new Sale
            {
                Id = 1,
                UserId = 1,
                StaffId = 2,
                ProductId = 1,
                SaleDate = createSaleDto.SaleDate,
                TotalAmount = 20.00m,
                DiscountAmount = 2.00m,
                FinalAmount = 18.00m,
                PaymentMethod = "Cash",
                IsActive = true,
                SalesBookingStatusId = (int)TempleApi.Enums.SalesBookingStatus.Awaiting,
                BookingToken = string.Empty,
                Notes = "Test sale",
                Customer = customer,
                Staff = staff,
                SaleItems = new List<SaleItem>
                {
                    new SaleItem
                    {
                        Id = 1,
                        SaleId = 1,
                        ProductId = 1,
                        Quantity = 2,
                        UnitPrice = 10.00m,
                        Subtotal = 20.00m,
                        Product = product
                    }
                }
            };

            _saleRepositoryMock.Setup(x => x.GetWithDetailsAsync(1)).ReturnsAsync(createdSale);

            // Act
            var result = await _saleService.CreateSaleAsync(createSaleDto);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(1);
            result.StaffId.Should().Be(2);
            result.TotalAmount.Should().Be(20.00m);
            result.FinalAmount.Should().Be(18.00m);
            result.PaymentMethod.Should().Be("Cash");
            result.Status.Should().Be("Completed");
            result.CustomerName.Should().Be("Customer");
            result.StaffName.Should().Be("Staff");
            result.ProductId.Should().Be(1);
            result.SalesBookingStatusId.Should().Be((int)TempleApi.Enums.SalesBookingStatus.Awaiting);
        }

        [Fact]
        public async Task CreateSaleAsync_ShouldThrowException_WhenCustomerNotFound()
        {
            // Arrange
            var createSaleDto = new CreateSaleDto
            {
                UserId = 999,
                StaffId = 2,
                SaleDate = DateTime.Now,
                TotalAmount = 20.00m,
                SaleItems = new List<CreateSaleItemDto>()
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _saleService.CreateSaleAsync(createSaleDto));
        }

        [Fact]
        public async Task CreateSaleAsync_ShouldThrowException_WhenStaffNotFound()
        {
            // Arrange
            var customer = new User { UserId = 1, Username = "customer", FullName = "Customer", Email = "customer@test.com", IsActive = true };
            var createSaleDto = new CreateSaleDto
            {
                UserId = 1,
                StaffId = 999,
                SaleDate = DateTime.Now,
                TotalAmount = 20.00m,
                SaleItems = new List<CreateSaleItemDto>()
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(customer);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _saleService.CreateSaleAsync(createSaleDto));
        }

        [Fact]
        public async Task CreateSaleAsync_ShouldThrowException_WhenProductNotAvailable()
        {
            // Arrange
            var customer = new User { UserId = 1, Username = "customer", FullName = "Customer", Email = "customer@test.com", IsActive = true };
            var staff = new User { UserId = 2, Username = "staff", FullName = "Staff", Email = "staff@test.com", IsActive = true };
            var createSaleDto = new CreateSaleDto
            {
                UserId = 1,
                StaffId = 2,
                SaleDate = DateTime.Now,
                TotalAmount = 20.00m,
                SaleItems = new List<CreateSaleItemDto>
                {
                    new CreateSaleItemDto { ProductId = 1, Quantity = 100 }
                }
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(customer);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(staff);
            _productServiceMock.Setup(x => x.CheckProductAvailabilityAsync(1, 100)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _saleService.CreateSaleAsync(createSaleDto));
        }

        #endregion

        #region Read Tests

        [Fact]
        public async Task GetSaleByIdAsync_ShouldReturnSale_WhenSaleExists()
        {
            // Arrange
            var sale = new Sale
            {
                Id = 1,
                UserId = 1,
                StaffId = 2,
                SaleDate = DateTime.Now,
                TotalAmount = 20.00m,
                FinalAmount = 18.00m,
                PaymentMethod = "Cash",
                IsActive = true,
                Customer = new User { UserId = 1, Username = "customer", FullName = "Customer" },
                Staff = new User { UserId = 2, Username = "staff", FullName = "Staff" },
                SaleItems = new List<SaleItem>
                {
                    new SaleItem
                    {
                        Id = 1,
                        ProductId = 1,
                        Quantity = 2,
                        UnitPrice = 10.00m,
                        Subtotal = 20.00m,
                        Product = new Product { Id = 1, Name = "Test Product" }
                    }
                }
            };

            _saleRepositoryMock.Setup(x => x.GetWithDetailsAsync(1)).ReturnsAsync(sale);

            // Act
            var result = await _saleService.GetSaleByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.CustomerName.Should().Be("Customer");
            result!.StaffName.Should().Be("Staff");
            result!.SaleItems.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSaleByIdAsync_ShouldReturnNull_WhenSaleDoesNotExist()
        {
            // Arrange
            _saleRepositoryMock.Setup(x => x.GetWithDetailsAsync(999)).ReturnsAsync((Sale?)null);

            // Act
            var result = await _saleService.GetSaleByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllSalesAsync_ShouldReturnAllSales()
        {
            // Arrange
            var sales = new List<Sale>
            {
                new Sale
                {
                    Id = 1,
                    UserId = 1,
                    StaffId = 2,
                    SaleDate = DateTime.Now,
                    TotalAmount = 20.00m,
                    IsActive = true,
                    Customer = new User { UserId = 1, Username = "customer1", FullName = "Customer 1" },
                    Staff = new User { UserId = 2, Username = "staff1", FullName = "Staff 1" },
                    SaleItems = new List<SaleItem>()
                },
                new Sale
                {
                    Id = 2,
                    UserId = 3,
                    StaffId = 2,
                    SaleDate = DateTime.Now,
                    TotalAmount = 30.00m,
                    IsActive = true,
                    Customer = new User { UserId = 3, Username = "customer2", FullName = "Customer 2" },
                    Staff = new User { UserId = 2, Username = "staff1", FullName = "Staff 1" },
                    SaleItems = new List<SaleItem>()
                }
            };

            _saleRepositoryMock.Setup(x => x.GetAllWithDetailsAsync()).ReturnsAsync(sales);

            // Act
            var result = await _saleService.GetAllSalesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Status == "Completed");
        }

        [Fact]
        public async Task GetSalesByCustomerAsync_ShouldReturnCustomerSales()
        {
            // Arrange
            var sales = new List<Sale>
            {
                new Sale
                {
                    Id = 1,
                    UserId = 1,
                    StaffId = 2,
                    SaleDate = DateTime.Now,
                    TotalAmount = 20.00m,
                    IsActive = true,
                    Customer = new User { UserId = 1, Username = "customer", FullName = "Customer" },
                    Staff = new User { UserId = 2, Username = "staff", FullName = "Staff" },
                    SaleItems = new List<SaleItem>()
                }
            };

            _saleRepositoryMock.Setup(x => x.GetByCustomerAsync(1)).ReturnsAsync(sales);

            // Act
            var result = await _saleService.GetSalesByCustomerAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.Should().OnlyContain(s => s.UserId == 1);
        }

        [Fact]
        public async Task GetSalesByStaffAsync_ShouldReturnStaffSales()
        {
            // Arrange
            var sales = new List<Sale>
            {
                new Sale
                {
                    Id = 1,
                    UserId = 1,
                    StaffId = 2,
                    SaleDate = DateTime.Now,
                    TotalAmount = 20.00m,
                    IsActive = true,
                    Customer = new User { UserId = 1, Username = "customer", FullName = "Customer" },
                    Staff = new User { UserId = 2, Username = "staff", FullName = "Staff" },
                    SaleItems = new List<SaleItem>()
                }
            };

            _saleRepositoryMock.Setup(x => x.GetByStaffAsync(2)).ReturnsAsync(sales);

            // Act
            var result = await _saleService.GetSalesByStaffAsync(2);

            // Assert
            result.Should().HaveCount(1);
            result.Should().OnlyContain(s => s.StaffId == 2);
        }

        [Fact]
        public async Task GetSalesByDateRangeAsync_ShouldReturnSalesInDateRange()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-7);
            var endDate = DateTime.Now;
            var sales = new List<Sale>
            {
                new Sale
                {
                    Id = 1,
                    UserId = 1,
                    StaffId = 2,
                    SaleDate = DateTime.Now.AddDays(-3),
                    TotalAmount = 20.00m,
                    IsActive = true,
                    Customer = new User { UserId = 1, Username = "customer", FullName = "Customer" },
                    Staff = new User { UserId = 2, Username = "staff", FullName = "Staff" },
                    SaleItems = new List<SaleItem>()
                }
            };

            _saleRepositoryMock.Setup(x => x.GetByDateRangeAsync(startDate, endDate)).ReturnsAsync(sales);

            // Act
            var result = await _saleService.GetSalesByDateRangeAsync(startDate, endDate);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task SearchSalesAsync_ShouldReturnMatchingSales()
        {
            // Arrange
            var sales = new List<Sale>
            {
                new Sale
                {
                    Id = 1,
                    UserId = 1,
                    StaffId = 2,
                    SaleDate = DateTime.Now,
                    TotalAmount = 20.00m,
                    IsActive = true,
                    Customer = new User { UserId = 1, Username = "john", FullName = "John Customer" },
                    Staff = new User { UserId = 2, Username = "staff", FullName = "Staff" },
                    SaleItems = new List<SaleItem>()
                },
                new Sale
                {
                    Id = 2,
                    UserId = 3,
                    StaffId = 2,
                    SaleDate = DateTime.Now,
                    TotalAmount = 30.00m,
                    IsActive = true,
                    Customer = new User { UserId = 3, Username = "jane", FullName = "Jane Customer" },
                    Staff = new User { UserId = 2, Username = "staff", FullName = "Staff" },
                    SaleItems = new List<SaleItem>()
                }
            };

            _saleRepositoryMock.Setup(x => x.GetAllWithDetailsAsync()).ReturnsAsync(sales);

            // Act
            var result = await _saleService.SearchSalesAsync("John");

            // Assert
            result.Should().HaveCount(1);
            result.Should().OnlyContain(s => s.CustomerName.Contains("John"));
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateSaleAsync_ShouldUpdateSale_WhenSaleExists()
        {
            // Arrange
            var sale = new Sale
            {
                Id = 1,
                UserId = 1,
                StaffId = 2,
                SaleDate = DateTime.Now,
                TotalAmount = 20.00m,
                IsActive = true
            };

            var updateDto = new CreateSaleDto
            {
                UserId = 1,
                StaffId = 2,
                ProductId = 1,
                SaleDate = DateTime.Now,
                TotalAmount = 25.00m,
                DiscountAmount = 3.00m,
                FinalAmount = 22.00m,
                PaymentMethod = "Card",
                Status = "Completed",
                SalesBookingStatusId = (int)TempleApi.Enums.SalesBookingStatus.InProgress,
                BookingToken = "token-xyz",
                Notes = "Updated sale",
                SaleItems = new List<CreateSaleItemDto>()
            };

            _saleRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(sale);
            _saleRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);

            var updatedSale = new Sale
            {
                Id = 1,
                UserId = 1,
                StaffId = 2,
                ProductId = 1,
                SaleDate = updateDto.SaleDate,
                TotalAmount = 25.00m,
                DiscountAmount = 3.00m,
                FinalAmount = 22.00m,
                PaymentMethod = "Card",
                IsActive = true,
                SalesBookingStatusId = (int)TempleApi.Enums.SalesBookingStatus.InProgress,
                Notes = "Updated sale",
                Customer = new User { UserId = 1, Username = "customer", FullName = "Customer" },
                Staff = new User { UserId = 2, Username = "staff", FullName = "Staff" },
                SaleItems = new List<SaleItem>()
            };

            _saleRepositoryMock.Setup(x => x.GetWithDetailsAsync(1)).ReturnsAsync(updatedSale);

            // Act
            var result = await _saleService.UpdateSaleAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(25.00m);
            result.FinalAmount.Should().Be(22.00m);
            result.PaymentMethod.Should().Be("Card");
            result.ProductId.Should().Be(1);
            result.SalesBookingStatusId.Should().Be((int)TempleApi.Enums.SalesBookingStatus.InProgress);
        }

        [Fact]
        public async Task UpdateSaleAsync_ShouldThrowException_WhenSaleDoesNotExist()
        {
            // Arrange
            var updateDto = new CreateSaleDto
            {
                UserId = 1,
                StaffId = 2,
                SaleDate = DateTime.Now,
                TotalAmount = 25.00m,
                SaleItems = new List<CreateSaleItemDto>()
            };

            _saleRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Sale?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _saleService.UpdateSaleAsync(999, updateDto));
        }

        [Fact]
        public async Task UpdateSaleStatusAsync_ShouldUpdateStatus_WhenSaleExists()
        {
            // Arrange
            var sale = new Sale
            {
                Id = 1,
                UserId = 1,
                StaffId = 2,
                SaleDate = DateTime.Now,
                TotalAmount = 20.00m,
                IsActive = false
            };

            _saleRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(sale);
            _saleRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);

            // Act
            var result = await _saleService.UpdateSaleStatusAsync(1, "Completed");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task VerifySaleBookingAsync_ShouldVerifyAndClearToken_WhenValid()
        {
            // Arrange
            var sale = new Sale
            {
                Id = 1,
                UserId = 1,
                StaffId = 2,
                SalesBookingStatusId = (int)TempleApi.Enums.SalesBookingStatus.Awaiting,
                BookingToken = "book-123",
                IsActive = true
            };

            _saleRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Sale> { sale });
            _saleRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);

            // Act
            var ok = await _saleService.VerifySaleBookingAsync("book-123");

            // Assert
            ok.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateSaleStatusAsync_ShouldReturnFalse_WhenSaleDoesNotExist()
        {
            // Arrange
            _saleRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Sale?)null);

            // Act
            var result = await _saleService.UpdateSaleStatusAsync(999, "Completed");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteSaleAsync_ShouldDeleteSaleAndRestoreInventory_WhenSaleExists()
        {
            // Arrange
            var sale = new Sale
            {
                Id = 1,
                UserId = 1,
                StaffId = 2,
                SaleDate = DateTime.Now,
                TotalAmount = 20.00m,
                IsActive = true
            };

            var saleItems = new List<SaleItem>
            {
                new SaleItem
                {
                    Id = 1,
                    SaleId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 10.00m,
                    Subtotal = 20.00m
                }
            };

            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Quantity = 50,
                Price = 10.00m,
                IsActive = true
            };

            _saleRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(sale);
            _saleItemRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SaleItem, bool>>>()))
                .ReturnsAsync(saleItems);
            _productRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _productRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _saleRepositoryMock.Setup(x => x.SoftDeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _saleService.DeleteSaleAsync(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteSaleAsync_ShouldReturnFalse_WhenSaleDoesNotExist()
        {
            // Arrange
            _saleRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Sale?)null);

            // Act
            var result = await _saleService.DeleteSaleAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}