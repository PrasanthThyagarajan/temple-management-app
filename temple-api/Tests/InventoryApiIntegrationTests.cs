using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Enums;
using TempleApi.Models.DTOs;
using Xunit;

namespace TempleApi.Tests
{
    public class InventoryApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly TempleDbContext _context;
        private readonly string _databaseName;

        public InventoryApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _databaseName = Guid.NewGuid().ToString();
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TempleDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<TempleDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(_databaseName);
                    });
                });
            });

            _client = _factory.CreateClient();
            _context = _factory.Services.GetRequiredService<TempleDbContext>();
        }

        public void Dispose()
        {
            _context.Dispose();
            _client.Dispose();
        }

        private async Task<(Temple temple, Area area)> SetupTestDataAsync()
        {
            var temple = new Temple
            {
                Name = "Test Temple",
                Address = "Test Address",
                City = "Test City",
                State = "Test State",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Temples.Add(temple);
            await _context.SaveChangesAsync();

            var area = new Area
            {
                TempleId = temple.Id,
                Name = "Test Area",
                Description = "Test Area Description",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            return (temple, area);
        }

        private async Task<string> GetAuthTokenAsync()
        {
            // Create a test user and get auth token
            var loginRequest = new
            {
                Username = "testuser",
                Password = "testpassword"
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            
            if (loginResponse.IsSuccessStatusCode)
            {
                var loginResult = await loginResponse.Content.ReadAsStringAsync();
                var loginData = JsonSerializer.Deserialize<JsonElement>(loginResult);
                return loginData.GetProperty("token").GetString();
            }

            // If login fails, create a user first
            var registerRequest = new
            {
                Username = "testuser",
                Password = "testpassword",
                Email = "test@example.com",
                Name = "Test User"
            };

            var registerJson = JsonSerializer.Serialize(registerRequest);
            var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");

            await _client.PostAsync("/api/auth/register", registerContent);

            // Try login again
            var retryLoginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            if (retryLoginResponse.IsSuccessStatusCode)
            {
                var loginResult = await retryLoginResponse.Content.ReadAsStringAsync();
                var loginData = JsonSerializer.Deserialize<JsonElement>(loginResult);
                return loginData.GetProperty("token").GetString();
            }

            return null;
        }

        private void SetAuthHeader(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        #region Create API Tests

        [Fact]
        public async Task CreateInventory_ShouldReturnCreated_WhenValidData()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var createDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 250.75m,
                Quantity = 10,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/inventories", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<InventoryDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().NotBeNull();
            result.ItemName.Should().Be(createDto.ItemName);
            result.TempleId.Should().Be(createDto.TempleId);
            result.AreaId.Should().Be(createDto.AreaId);
            result.ItemWorth.Should().Be(createDto.ItemWorth);
            result.ApproximatePrice.Should().Be(createDto.ApproximatePrice);
            result.Quantity.Should().Be(createDto.Quantity);
            result.Active.Should().Be(createDto.Active);
        }

        [Fact]
        public async Task CreateInventory_ShouldReturnBadRequest_WhenTempleNotFound()
        {
            // Arrange
            var (_, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var createDto = new CreateInventoryDto
            {
                TempleId = 999, // Non-existent temple
                AreaId = area.Id,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/inventories", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateInventory_ShouldReturnBadRequest_WhenAreaNotFound()
        {
            // Arrange
            var (temple, _) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var createDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = 999, // Non-existent area
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/inventories", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Read API Tests

        [Fact]
        public async Task GetAllInventories_ShouldReturnOk_WithInventories()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var inventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/inventories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<InventoryDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().HaveCount(1);
            result[0].ItemName.Should().Be("Test Item");
        }

        [Fact]
        public async Task GetInventoryById_ShouldReturnOk_WhenExists()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var inventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 200.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/inventories/{inventory.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<InventoryDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().NotBeNull();
            result.Id.Should().Be(inventory.Id);
            result.ItemName.Should().Be("Test Item");
        }

        [Fact]
        public async Task GetInventoryById_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            // Act
            var response = await _client.GetAsync("/api/inventories/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetInventoriesByTemple_ShouldReturnOk_WithFilteredResults()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var inventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Temple Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/inventories/temple/{temple.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<InventoryDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().HaveCount(1);
            result[0].TempleId.Should().Be(temple.Id);
        }

        [Fact]
        public async Task GetInventoriesByArea_ShouldReturnOk_WithFilteredResults()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var inventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Area Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 50.00m,
                Quantity = 10,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/inventories/area/{area.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<InventoryDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().HaveCount(1);
            result[0].AreaId.Should().Be(area.Id);
        }

        [Fact]
        public async Task GetInventoriesByWorth_ShouldReturnOk_WithFilteredResults()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var preciousInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Precious Item",
                ItemWorth = ItemWorth.Precious,
                ApproximatePrice = 1000.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(preciousInventory);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/inventories/worth/{ItemWorth.Precious}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<InventoryDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().HaveCount(1);
            result[0].ItemWorth.Should().Be(ItemWorth.Precious);
        }

        [Fact]
        public async Task GetActiveInventories_ShouldReturnOk_WithActiveItemsOnly()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var activeInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Active Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var inactiveInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Inactive Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 50.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = false,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.AddRange(activeInventory, inactiveInventory);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/inventories/active");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<InventoryDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().HaveCount(1);
            result[0].Active.Should().BeTrue();
        }

        #endregion

        #region Update API Tests

        [Fact]
        public async Task UpdateInventory_ShouldReturnOk_WhenValidData()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var inventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Original Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            var updateDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Updated Item",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 200.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                Active = false
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/inventories/{inventory.Id}", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<InventoryDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().NotBeNull();
            result.ItemName.Should().Be("Updated Item");
            result.ItemWorth.Should().Be(ItemWorth.High);
            result.ApproximatePrice.Should().Be(200.00m);
            result.Quantity.Should().Be(3);
            result.Active.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateInventory_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var updateDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Updated Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("/api/inventories/999", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Delete API Tests

        [Fact]
        public async Task DeleteInventory_ShouldReturnOk_WhenExists()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var inventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item to Delete",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 25.00m,
                Quantity = 2,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/inventories/{inventory.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            result.GetProperty("message").GetString().Should().Be("Inventory item deleted successfully");

            // Verify deletion
            var deletedInventory = await _context.Inventories.FindAsync(inventory.Id);
            deletedInventory.Should().BeNull();
        }

        [Fact]
        public async Task DeleteInventory_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            // Act
            var response = await _client.DeleteAsync("/api/inventories/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Calculation API Tests

        [Fact]
        public async Task GetTotalValueByArea_ShouldReturnOk_WithCorrectValue()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var inventory1 = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item 1",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 2,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var inventory2 = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item 2",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 50.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.AddRange(inventory1, inventory2);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/inventories/area/{area.Id}/value");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            result.GetProperty("areaId").GetInt32().Should().Be(area.Id);
            result.GetProperty("totalValue").GetDecimal().Should().Be(350.00m); // (100*2) + (50*3)
        }

        [Fact]
        public async Task GetTotalQuantityByTemple_ShouldReturnOk_WithCorrectQuantity()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var token = await GetAuthTokenAsync();
            SetAuthHeader(token);

            var inventory1 = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item 1",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var inventory2 = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item 2",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 25.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.AddRange(inventory1, inventory2);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/inventories/temple/{temple.Id}/quantity");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            result.GetProperty("templeId").GetInt32().Should().Be(temple.Id);
            result.GetProperty("totalQuantity").GetInt32().Should().Be(8); // 5 + 3
        }

        #endregion

        #region Authorization Tests

        [Fact]
        public async Task CreateInventory_ShouldReturnUnauthorized_WhenNoToken()
        {
            // Arrange
            var createDto = new CreateInventoryDto
            {
                TempleId = 1,
                AreaId = 1,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/inventories", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllInventories_ShouldReturnUnauthorized_WhenNoToken()
        {
            // Act
            var response = await _client.GetAsync("/api/inventories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion
    }
}
