using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IApplicationUserService
    {
        Task<Result<ApplicationUser>> UpdateUserAsync(string userId, UpdateProfileDto dto);

        Task<Result<ApplicationUser>> GetSellerProfileAsync(string userId);

        Task<Result> UpdateSellerDescriptionAsync(string userId, string? description);
    }
}