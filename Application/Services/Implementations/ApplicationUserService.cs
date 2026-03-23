using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Application.Services.Interfaces;

namespace MadeByMe.Application.Services.Implementations
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IUserRepository _userRepository;

        public ApplicationUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ApplicationUser UpdateUser(string userId, UpdateProfileDto dto)
        {
            var user = _userRepository.GetById(userId);

            if (user != null)
            {
                user.UserName = dto.UserName ?? user.UserName;
                user.Email = dto.Email ?? user.Email;
                user.ProfilePicture = dto.ProfilePicture ?? user.ProfilePicture;
                user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
                user.Address = dto.Address ?? user.Address;

                _userRepository.Update(user);
            }

            return user!;
        }
    }
}