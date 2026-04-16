using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{

    public interface IAdminService
    {
        Task<List<ApplicationUser>> GetUsersAsync();

        Task BlockUserAsync(string userId);

        Task UnblockUserAsync(string userId);

        Task DeleteUserAsync(string userId);
    }
}