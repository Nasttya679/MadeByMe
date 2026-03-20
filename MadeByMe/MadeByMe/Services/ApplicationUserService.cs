using MadeByMe.Models;
using MadeByMe.DTOs;
using MadeByMe.Data;

namespace MadeByMe.Services
{
    public class ApplicationUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationUserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationUser? UpdateUser(string userId, UpdateProfileDto dto)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.UserName = dto.UserName ?? user.UserName;
                user.Email = dto.Email ?? user.Email;
                user.ProfilePicture = dto.ProfilePicture ?? user.ProfilePicture;
                user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
                user.Address = dto.Address ?? user.Address;
                _context.SaveChanges();
            }

            return user;
        }
    }
}
