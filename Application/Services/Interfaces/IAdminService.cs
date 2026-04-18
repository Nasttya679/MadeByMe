using MadeByMe.Application.Common;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IAdminService
    {
        Task<Result<List<ApplicationUser>>> GetUsersAsync();

        Task<Result> BlockUserAsync(string userId);

        Task<Result> UnblockUserAsync(string userId);

        Task<Result> DeleteUserAsync(string userId);
    }
}