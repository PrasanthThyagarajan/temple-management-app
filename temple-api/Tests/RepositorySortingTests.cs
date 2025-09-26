using Xunit;
using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories;
using TempleApi.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace TempleApi.Tests
{
    public class RepositorySortingTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly Repository<Temple> _templeRepository;
        private readonly Repository<Area> _areaRepository;

        public RepositorySortingTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _context = new TempleDbContext(options);
            _templeRepository = new Repository<Temple>(_context);
            _areaRepository = new Repository<Area>(_context);
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test temples with different creation times
            _context.Temples.AddRange(
                new Temple { Id = 1, Name = "Old Temple", City = "City1", State = "State1", Deity = "Deity1", IsActive = true },
                new Temple { Id = 5, Name = "Middle Temple", City = "City2", State = "State2", Deity = "Deity2", IsActive = true },
                new Temple { Id = 10, Name = "New Temple", City = "City3", State = "State3", Deity = "Deity3", IsActive = true },
                new Temple { Id = 3, Name = "Inactive Temple", City = "City4", State = "State4", Deity = "Deity4", IsActive = false }
            );

            // Add test areas
            _context.Areas.AddRange(
                new Area { Id = 2, Name = "Area B", TempleId = 1, Description = "Description B", IsActive = true },
                new Area { Id = 7, Name = "Area G", TempleId = 5, Description = "Description G", IsActive = true },
                new Area { Id = 4, Name = "Area D", TempleId = 10, Description = "Description D", IsActive = true },
                new Area { Id = 9, Name = "Area I", TempleId = 1, Description = "Description I", IsActive = true }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsActiveEntitiesInDescendingOrder()
        {
            // Act
            var temples = await _templeRepository.GetAllAsync();
            var templeList = temples.ToList();

            // Assert
            Assert.Equal(3, templeList.Count); // Only active temples
            Assert.Equal(10, templeList[0].Id); // Highest ID first
            Assert.Equal(5, templeList[1].Id);
            Assert.Equal(1, templeList[2].Id); // Lowest ID last
            Assert.DoesNotContain(templeList, t => t.Id == 3); // Inactive temple excluded
        }

        [Fact]
        public async Task GetAllAsync_WithIncludes_ReturnsInDescendingOrder()
        {
            // Act
            var areas = await _areaRepository.GetAllAsync(a => a.Temple);
            var areaList = areas.ToList();

            // Assert
            Assert.Equal(4, areaList.Count);
            Assert.Equal(9, areaList[0].Id); // Highest ID first
            Assert.Equal(7, areaList[1].Id);
            Assert.Equal(4, areaList[2].Id);
            Assert.Equal(2, areaList[3].Id); // Lowest ID last
            Assert.NotNull(areaList[0].Temple); // Include worked
        }

        [Fact]
        public async Task FindAsync_ReturnsFilteredResultsInDescendingOrder()
        {
            // Act
            var areas = await _areaRepository.FindAsync(a => a.TempleId == 1);
            var areaList = areas.ToList();

            // Assert
            Assert.Equal(2, areaList.Count); // Areas for Temple 1
            Assert.Equal(9, areaList[0].Id); // Area 9 first
            Assert.Equal(2, areaList[1].Id); // Area 2 last
        }

        [Fact]
        public async Task FindAsync_WithIncludes_ReturnsFilteredResultsInDescendingOrder()
        {
            // Act
            var areas = await _areaRepository.FindAsync(
                a => a.Description.Contains("Description"), 
                a => a.Temple
            );
            var areaList = areas.ToList();

            // Assert
            Assert.Equal(4, areaList.Count); // All areas have "Description"
            Assert.Equal(9, areaList[0].Id); // Highest ID first
            Assert.NotNull(areaList[0].Temple); // Include worked
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResultsInDescendingOrder()
        {
            // Act
            var firstPage = await _areaRepository.GetPagedAsync(1, 2);
            var secondPage = await _areaRepository.GetPagedAsync(2, 2);
            
            var firstPageList = firstPage.ToList();
            var secondPageList = secondPage.ToList();

            // Assert
            Assert.Equal(2, firstPageList.Count);
            Assert.Equal(2, secondPageList.Count);
            
            // First page should have highest IDs
            Assert.Equal(9, firstPageList[0].Id);
            Assert.Equal(7, firstPageList[1].Id);
            
            // Second page should have lower IDs
            Assert.Equal(4, secondPageList[0].Id);
            Assert.Equal(2, secondPageList[1].Id);
        }

        [Fact]
        public async Task GetPagedAsync_WithPredicate_ReturnsFilteredPagedResultsInDescendingOrder()
        {
            // Act
            var pagedAreas = await _areaRepository.GetPagedAsync(
                1, 
                10, 
                a => a.Name.Contains("Area")
            );
            var areaList = pagedAreas.ToList();

            // Assert
            Assert.Equal(4, areaList.Count);
            Assert.Equal(9, areaList[0].Id); // Highest ID first
            Assert.Equal(7, areaList[1].Id);
            Assert.Equal(4, areaList[2].Id);
            Assert.Equal(2, areaList[3].Id); // Lowest ID last
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
