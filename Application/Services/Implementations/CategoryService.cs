using System.Collections.Generic;
using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;

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
            return Result<List<Category>>.Success(categories);
        }

        public Result<Category> GetCategoryById(int id)
        {
            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                return Result<Category>.Failure($"Категорію з ID {id} не знайдено.");
            }

            return Result<Category>.Success(category);
        }

        public Result<Category> CreateCategory(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
            };

            _categoryRepository.Add(category);
            return Result<Category>.Success(category);
        }

        public Result<Category> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                return Result<Category>.Failure("Категорію для оновлення не знайдено.");
            }

            category.Name = dto.Name;
            _categoryRepository.Update(category);

            return Result<Category>.Success(category);
        }

        public Result DeleteCategory(int id)
        {
            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                return Result.Failure("Категорію для видалення не знайдено.");
            }

            _categoryRepository.Delete(category);
            return Result.Success();
        }
    }
}