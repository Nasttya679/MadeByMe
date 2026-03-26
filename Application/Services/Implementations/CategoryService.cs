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

        public Result<List<Category>> GetAllCategories()
        {
            var categories = _categoryRepository.GetAll();
            Log.Information("Отримано список усіх категорій. Кількість записів: {Count}", categories.Count);
            return Result<List<Category>>.Success(categories);
        }

        public Result<Category> GetCategoryById(int id)
        {
            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                Log.Warning("Категорію з ідентифікатором {CategoryId} не знайдено в базі даних", id);
                return Result<Category>.Failure($"Категорію з ID {id} не знайдено.");
            }

            return Result<Category>.Success(category);
        }

        public Result<Category> CreateCategory(CreateCategoryDto dto)
        {
            Log.Information("Створення нової категорії: {CategoryName}", dto.Name);

            var category = new Category
            {
                Name = dto.Name,
            };

            _categoryRepository.Add(category);

            Log.Information("Категорію '{CategoryName}' успішно створено з ID {CategoryId}", dto.Name, category.CategoryId);
            return Result<Category>.Success(category);
        }

        public Result<Category> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            Log.Information("Оновлення категорії з ID {CategoryId}", id);

            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                Log.Warning("Невдала спроба оновлення: категорію {CategoryId} не знайдено", id);
                return Result<Category>.Failure("Категорію для оновлення не знайдено.");
            }

            var oldName = category.Name;
            category.Name = dto.Name;

            _categoryRepository.Update(category);

            Log.Information("Категорію {CategoryId} оновлено. '{OldName}' -> '{NewName}'", id, oldName, dto.Name);
            return Result<Category>.Success(category);
        }

        public Result DeleteCategory(int id)
        {
            Log.Information("Видалення категорії з ID {CategoryId}", id);

            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                Log.Warning("Невдала спроба видалення: категорію {CategoryId} не знайдено", id);
                return Result.Failure("Категорію для видалення не знайдено.");
            }

            _categoryRepository.Delete(category);

            Log.Information("Категорію '{CategoryName}' (ID: {CategoryId}) успішно видалено", category.Name, id);
            return Result.Success();
        }
    }
}