using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Enums;
using TempleApi.Models.DTOs;
using TempleApi.Repositories;
using TempleApi.Repositories.Interfaces;
using TempleApi.Services;
using TempleApi.Services.Interfaces;
using Xunit;

namespace TempleApi.Tests
{
    public class CategoryServiceTests
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryServiceTests()
        {
            var databaseName = Guid.NewGuid().ToString();
            var testDbContextFactory = new TestDbContextFactory(databaseName);

            _categoryRepository = new CategoryRepository(testDbContextFactory);
            _categoryService = new CategoryService((ICategoryRepository)_categoryRepository);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCreate_WhenValid()
        {
            var dto = new CreateCategoryDto
            {
                Name = "Puja Items",
                Description = "Items used for puja",
                IsActive = true,
                SortOrder = 1
            };

            var created = await _categoryService.CreateCategoryAsync(dto);

            created.Should().NotBeNull();
            created.Id.Should().BeGreaterThan(0);
            created.Name.Should().Be(dto.Name);
            created.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrow_WhenDuplicateName()
        {
            var dto = new CreateCategoryDto { Name = "Books", Description = "Temple books", IsActive = true, SortOrder = 0 };
            await _categoryService.CreateCategoryAsync(dto);

            var act = async () => await _categoryService.CreateCategoryAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAll()
        {
            await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "Clothing", IsActive = true });
            await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "Jewelry", IsActive = false });

            var all = await _categoryService.GetAllCategoriesAsync();
            all.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetActiveCategoriesAsync_ShouldReturnOnlyActive()
        {
            await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "Food", IsActive = true });
            await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "Decor", IsActive = false });

            var active = await _categoryService.GetActiveCategoriesAsync();
            active.Should().OnlyContain(c => c.IsActive);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldUpdateFields_WhenExists()
        {
            var created = await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "Other", Description = "Misc", IsActive = true, SortOrder = 2 });

            var updated = await _categoryService.UpdateCategoryAsync(created.Id, new CreateCategoryDto
            {
                Name = "Miscellaneous",
                Description = "Updated",
                IsActive = false,
                SortOrder = 5
            });

            updated.Should().NotBeNull();
            updated!.Name.Should().Be("Miscellaneous");
            updated.IsActive.Should().BeFalse();
            updated.SortOrder.Should().Be(5);
            updated.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnNull_WhenNotFound()
        {
            var updated = await _categoryService.UpdateCategoryAsync(9999, new CreateCategoryDto { Name = "X" });
            updated.Should().BeNull();
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrow_WhenDuplicateName()
        {
            var c1 = await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "A" });
            var c2 = await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "B" });

            var act = async () => await _categoryService.UpdateCategoryAsync(c2.Id, new CreateCategoryDto { Name = "A" });
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task ToggleCategoryStatusAsync_ShouldToggle()
        {
            var c = await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "ToggleCat", IsActive = true });

            var res1 = await _categoryService.ToggleCategoryStatusAsync(c.Id);
            res1.Should().BeTrue();
            var after1 = await _categoryService.GetCategoryByIdAsync(c.Id);
            after1!.IsActive.Should().BeFalse();

            var res2 = await _categoryService.ToggleCategoryStatusAsync(c.Id);
            res2.Should().BeTrue();
            var after2 = await _categoryService.GetCategoryByIdAsync(c.Id);
            after2!.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldDelete_WhenExists()
        {
            var c = await _categoryService.CreateCategoryAsync(new CreateCategoryDto { Name = "DeleteMe" });
            var deleted = await _categoryService.DeleteCategoryAsync(c.Id);
            deleted.Should().BeTrue();

            var again = await _categoryService.DeleteCategoryAsync(c.Id);
            again.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CreateCategoryAsync_ShouldValidate_NameRequired(string? name)
        {
            var dto = new CreateCategoryDto { Name = name ?? string.Empty };
            var act = async () => await _categoryService.CreateCategoryAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldValidate_NameMaxLength()
        {
            var dto = new CreateCategoryDto { Name = new string('x', 101) };
            var act = async () => await _categoryService.CreateCategoryAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot exceed 100 characters*");
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldValidate_DescriptionMaxLength()
        {
            var dto = new CreateCategoryDto { Name = "Valid", Description = new string('y', 501) };
            var act = async () => await _categoryService.CreateCategoryAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot exceed 500 characters*");
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldValidate_SortOrderNonNegative()
        {
            var dto = new CreateCategoryDto { Name = "Valid", SortOrder = -1 };
            var act = async () => await _categoryService.CreateCategoryAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be negative*");
        }

        private class TestDbContextFactory : IDbContextFactory
        {
            private readonly DbContextOptions<TempleDbContext> _options;

            public TestDbContextFactory(string databaseName)
            {
                _options = new DbContextOptionsBuilder<TempleDbContext>()
                    .UseInMemoryDatabase(databaseName)
                    .Options;
            }

            public DbContext CreateDbContext(DatabaseProvider provider, string connectionString)
            {
                return new TempleDbContext(_options);
            }

            public TempleDbContext CreateTempleDbContext()
            {
                return new TempleDbContext(_options);
            }
        }
    }
}


