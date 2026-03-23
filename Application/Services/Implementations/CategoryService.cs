using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Application.Services.Interfaces;
using System.Collections.Generic;

namespace MadeByMe.Application.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public List<Category> GetAllCategories()
        {
            return _categoryRepository.GetAll();
        }

        public Category GetCategoryById(int id)
        {
            return _categoryRepository.GetById(id)!; // ! щоб сказати: точно не null
        }

        public Category CreateCategory(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name
            };

            _categoryRepository.Add(category);
            return category;
        }

        public Category UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var category = _categoryRepository.GetById(id);
            if (category != null)
            {
                category.Name = dto.Name;
                _categoryRepository.Update(category);
            }
            return category!;
        }

        public bool DeleteCategory(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category != null)
            {
                _categoryRepository.Delete(category);
                return true;
            }
            return false;
        }
    }
}