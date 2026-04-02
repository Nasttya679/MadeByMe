using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<List<Category>>> GetAllCategoriesAsync();

        Task<Result<Category>> GetCategoryByIdAsync(int id);

        Task<Result<Category>> CreateCategoryAsync(CreateCategoryDto dto);

        Task<Result<Category>> UpdateCategoryAsync(int id, UpdateCategoryDto dto);

        Task<Result> DeleteCategoryAsync(int id);
    }
}