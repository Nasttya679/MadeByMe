
using MadeByMe.Domain.Entities;


namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        ApplicationUser? GetById(string userId);
        
        void Update(ApplicationUser user);
    }
}
