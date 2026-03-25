using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;

namespace MadeByMe.Application.Services.Implementations
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IUserRepository _userRepository;

        public ApplicationUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Result<ApplicationUser> UpdateUser(string userId, UpdateProfileDto dto)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                return Result<ApplicationUser>.Failure("Користувача з таким ідентифікатором не знайдено.");
            }

            user.UserName = dto.UserName ?? user.UserName;
            user.Email = dto.Email ?? user.Email;
            user.ProfilePicture = dto.ProfilePicture ?? user.ProfilePicture;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            user.Address = dto.Address ?? user.Address;

            _userRepository.Update(user);

            return Result<ApplicationUser>.Success(user);
        }
    }
}