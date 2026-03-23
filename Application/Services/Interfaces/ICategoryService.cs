
using MadeByMe.Domain.Entities;
using MadeByMe.Application.DTOs;


namespace MadeByMe.Application.Services.Interfaces {
    public interface ICategoryService
    {
        List<Category> GetAllCategories();
        Category GetCategoryById(int id);
        Category CreateCategory(CreateCategoryDto dto);
        Category UpdateCategory(int id, UpdateCategoryDto dto);
        bool DeleteCategory(int id);
    }
}