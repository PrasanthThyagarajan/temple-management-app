using Xunit;
using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Services;
using TempleApi.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using TempleApi.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace TempleApi.Tests
{
    public class SortingTests : IDisposable
    {
        private readonly TempleDbContext _context;

        public SortingTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _context = new TempleDbContext(options);
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test temples
            _context.Temples.AddRange(
                new Temple { Id = 1, Name = "Temple A", City = "City1", State = "State1", Deity = "Deity1", IsActive = true },
                new Temple { Id = 2, Name = "Temple B", City = "City2", State = "State2", Deity = "Deity2", IsActive = true },
                new Temple { Id = 3, Name = "Temple C", City = "City3", State = "State3", Deity = "Deity3", IsActive = true }
            );

            // Add test areas
            _context.Areas.AddRange(
                new Area { Id = 1, Name = "Area A", TempleId = 1, Description = "Description 1", IsActive = true },
                new Area { Id = 2, Name = "Area B", TempleId = 1, Description = "Description 2", IsActive = true },
                new Area { Id = 3, Name = "Area C", TempleId = 2, Description = "Description 3", IsActive = true }
            );

            // Add test devotees
            _context.Devotees.AddRange(
                new Devotee { Id = 1, FullName = "John Doe", Email = "john@test.com", Phone = "1234567890", TempleId = 1, IsActive = true },
                new Devotee { Id = 2, FullName = "Jane Smith", Email = "jane@test.com", Phone = "0987654321", TempleId = 1, IsActive = true },
                new Devotee { Id = 3, FullName = "Bob Johnson", Email = "bob@test.com", Phone = "5555555555", TempleId = 2, IsActive = true }
            );

            // Add test events
            _context.Events.AddRange(
                new Event { Id = 1, Name = "Event A", AreaId = 1, EventTypeId = 1, StartDate = DateTime.Now.AddDays(-5), EndDate = DateTime.Now.AddDays(-4), IsActive = true },
                new Event { Id = 2, Name = "Event B", AreaId = 2, EventTypeId = 1, StartDate = DateTime.Now.AddDays(-3), EndDate = DateTime.Now.AddDays(-2), IsActive = true },
                new Event { Id = 3, Name = "Event C", AreaId = 3, EventTypeId = 1, StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now, IsActive = true }
            );

            // Add test event types
            _context.EventTypes.AddRange(
                new EventType { Id = 1, Name = "Type A" },
                new EventType { Id = 2, Name = "Type B" },
                new EventType { Id = 3, Name = "Type C" }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllTemplesAsync_ReturnsTemplesInDescendingOrder()
        {
            // Arrange
            var service = new TempleService(_context);

            // Act
            var temples = await service.GetAllTemplesAsync();
            var templeList = temples.ToList();

            // Assert
            Assert.Equal(3, templeList.Count);
            Assert.Equal(3, templeList[0].Id); // Newest first
            Assert.Equal(2, templeList[1].Id);
            Assert.Equal(1, templeList[2].Id); // Oldest last
        }

        [Fact]
        public async Task GetAllAreasAsync_ReturnsAreasInDescendingOrder()
        {
            // Arrange
            var service = new AreaService(_context);

            // Act
            var areas = await service.GetAllAreasAsync();
            var areaList = areas.ToList();

            // Assert
            Assert.Equal(3, areaList.Count);
            Assert.Equal(3, areaList[0].Id); // Newest first
            Assert.Equal(2, areaList[1].Id);
            Assert.Equal(1, areaList[2].Id); // Oldest last
        }

        [Fact]
        public async Task GetAllDevoteesAsync_ReturnsDevoteesInDescendingOrder()
        {
            // Arrange
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
            var logger = new NullLogger<DevoteeService>();
            var service = new DevoteeService(_context, config, logger);

            // Act
            var devotees = await service.GetAllDevoteesAsync();
            var devoteeList = devotees.ToList();

            // Assert
            Assert.Equal(3, devoteeList.Count);
            Assert.Equal(3, devoteeList[0].Id); // Newest first
            Assert.Equal(2, devoteeList[1].Id);
            Assert.Equal(1, devoteeList[2].Id); // Oldest last
        }

        [Fact]
        public async Task GetAllEventsAsync_ReturnsEventsInDescendingOrder()
        {
            // Arrange
            var service = new EventService(_context);

            // Act
            var events = await service.GetAllEventsAsync();
            var eventList = events.ToList();

            // Assert
            Assert.Equal(3, eventList.Count);
            Assert.Equal(3, eventList[0].Id); // Newest first
            Assert.Equal(2, eventList[1].Id);
            Assert.Equal(1, eventList[2].Id); // Oldest last
        }

        [Fact]
        public async Task GetAllEventTypesAsync_ReturnsEventTypesInDescendingOrder()
        {
            // Arrange
            var service = new EventTypeService(_context);

            // Act
            var eventTypes = await service.GetAllAsync();
            var typeList = eventTypes.ToList();

            // Assert
            Assert.Equal(3, typeList.Count);
            Assert.Equal(3, typeList[0].Id); // Newest first
            Assert.Equal(2, typeList[1].Id);
            Assert.Equal(1, typeList[2].Id); // Oldest last
        }

        [Fact]
        public async Task SearchTemplesAsync_ReturnsResultsInDescendingOrder()
        {
            // Arrange
            var service = new TempleService(_context);

            // Act
            var temples = await service.SearchTemplesAsync("Temple");
            var templeList = temples.ToList();

            // Assert
            Assert.Equal(3, templeList.Count);
            Assert.Equal(3, templeList[0].Id); // Newest first
            Assert.Equal(2, templeList[1].Id);
            Assert.Equal(1, templeList[2].Id); // Oldest last
        }

        [Fact]
        public async Task GetAreasByTempleAsync_ReturnsAreasInDescendingOrder()
        {
            // Arrange
            var service = new AreaService(_context);

            // Act
            var areas = await service.GetAreasByTempleAsync(1);
            var areaList = areas.ToList();

            // Assert
            Assert.Equal(2, areaList.Count); // Temple 1 has 2 areas
            Assert.Equal(2, areaList[0].Id); // Area 2 first
            Assert.Equal(1, areaList[1].Id); // Area 1 last
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
