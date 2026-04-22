using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private const string CategoriesCacheKey = "AllCategoriesList";

        private readonly ICategoryRepository _categoryRepository;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _categoryRepository = categoryRepository;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<Result<List<Category>>> GetAllCategoriesAsync()
        {
            if (!_cache.TryGetValue(CategoriesCacheKey, out List<Category>? categories) || categories == null)
            {
                categories = await _categoryRepository.GetAllAsync();

                var cacheMinutes = _configuration.GetValue<int>("CacheSettings:CategoriesCacheDurationMinutes", 30);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheMinutes))
                    .SetPriority(CacheItemPriority.High);

                _cache.Set(CategoriesCacheKey, categories, cacheOptions);

                Log.Information(" -- Дані про категорії завантажено з БД і збережено в КЕШ на {CacheMinutes} хв --. Кількість: {Count}", cacheMinutes, categories.Count);
            }
            else
            {
                Log.Information(" -- Дані про категорії отримано з КЕШУ --");
            }

            return categories;
        }

        public async Task<Result<Category>> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                Log.Warning("Категорію з ідентифікатором {CategoryId} не знайдено в базі даних", id);
                return $"Категорію з ID {id} не знайдено.";
            }

            return category;
        }

        public async Task<Result<Category>> CreateCategoryAsync(CreateCategoryDto dto)
        {
            Log.Information("Створення нової категорії: {CategoryName}", dto.Name);

            var category = new Category
            {
                Name = dto.Name,
            };

            await _categoryRepository.AddAsync(category);

            _cache.Remove(CategoriesCacheKey);

            Log.Information("Категорію '{CategoryName}' успішно створено з ID {CategoryId}. Кеш скинуто.", dto.Name, category.CategoryId);

            return category;
        }

        public async Task<Result<Category>> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            Log.Information("Оновлення категорії з ID {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                Log.Warning("Невдала спроба оновлення: категорію {CategoryId} не знайдено", id);
                return "Категорію для оновлення не знайдено.";
            }

            var oldName = category.Name;
            category.Name = dto.Name;

            await _categoryRepository.UpdateAsync(category);

            _cache.Remove(CategoriesCacheKey);

            Log.Information("Категорію {CategoryId} оновлено. '{OldName}' -> '{NewName}'. Кеш скинуто.", id, oldName, dto.Name);

            return category;
        }

        public async Task<Result> DeleteCategoryAsync(int id)
        {
            Log.Information("Видалення категорії з ID {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                Log.Warning("Невдала спроба видалення: категорію {CategoryId} не знайдено", id);
                return "Категорію для видалення не знайдено.";
            }

            await _categoryRepository.DeleteAsync(category);

            _cache.Remove(CategoriesCacheKey);

            Log.Information("Категорію '{CategoryName}' (ID: {CategoryId}) успішно видалено. Кеш скинуто.", category.Name, id);

            return Result.Success();
        }
    }
}