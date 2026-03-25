using System.Collections.Generic;
using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Result<List<Category>> GetAllCategories();

        Result<Category> GetCategoryById(int id);

        Result<Category> CreateCategory(CreateCategoryDto dto);

        Result<Category> UpdateCategory(int id, UpdateCategoryDto dto);

        Result DeleteCategory(int id);
    }
}