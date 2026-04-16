using MadeByMe.Domain.Entities;


namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string userId);

        Task<List<ApplicationUser>> GetAllExceptAdminsAsync();
        
        Task UpdateAsync(ApplicationUser user);

        Task DeleteAsync(ApplicationUser user);
    }
}
