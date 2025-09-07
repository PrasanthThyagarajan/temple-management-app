using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Repositories.Interfaces;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category != null ? MapToDto(category) : null;
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync();
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            ValidateCreateDto(createDto);

            // Check if category with same name already exists
            var existingCategory = await _categoryRepository.GetByNameAsync(createDto.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Category with name '{createDto.Name}' already exists.");
            }

            var category = new Category
            {
                Name = createDto.Name,
                Description = createDto.Description,
                IsActive = createDto.IsActive,
                SortOrder = createDto.SortOrder
            };

            await _categoryRepository.AddAsync(category);
            return MapToDto(category);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryDto updateDto)
        {
            ValidateCreateDto(updateDto);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            // Check if another category with same name already exists
            var existingCategory = await _categoryRepository.GetByNameAsync(updateDto.Name);
            if (existingCategory != null && existingCategory.Id != id)
            {
                throw new InvalidOperationException($"Category with name '{updateDto.Name}' already exists.");
            }

            category.Name = updateDto.Name;
            category.Description = updateDto.Description;
            category.IsActive = updateDto.IsActive;
            category.SortOrder = updateDto.SortOrder;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
            return MapToDto(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            await _categoryRepository.DeleteAsync(category);
            return true;
        }

        public async Task<bool> ToggleCategoryStatusAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
            return true;
        }

        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        private static void ValidateCreateDto(CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(dto.Name));
            }

            if (dto.Name.Length > 100)
            {
                throw new ArgumentException("Category name cannot exceed 100 characters.", nameof(dto.Name));
            }

            if (dto.Description.Length > 500)
            {
                throw new ArgumentException("Category description cannot exceed 500 characters.", nameof(dto.Description));
            }

            if (dto.SortOrder < 0)
            {
                throw new ArgumentException("Sort order cannot be negative.", nameof(dto.SortOrder));
            }
        }
    }
}
