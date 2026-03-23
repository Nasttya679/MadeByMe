using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;


namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public ApplicationUser? GetById(string userId)
        {
            return _context.Users.Find(userId);
        }

        public void Update(ApplicationUser user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
    }
}