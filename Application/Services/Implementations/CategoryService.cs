using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<List<Category>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            Log.Information("Отримано список усіх категорій. Кількість записів: {Count}", categories.Count);

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

            Log.Information("Категорію '{CategoryName}' успішно створено з ID {CategoryId}", dto.Name, category.CategoryId);

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

            Log.Information("Категорію {CategoryId} оновлено. '{OldName}' -> '{NewName}'", id, oldName, dto.Name);

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

            Log.Information("Категорію '{CategoryName}' (ID: {CategoryId}) успішно видалено", category.Name, id);

            return Result.Success();
        }
    }
}