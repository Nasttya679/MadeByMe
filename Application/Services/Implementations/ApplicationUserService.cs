using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IUserRepository _userRepository;

        public ApplicationUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<ApplicationUser>> UpdateUserAsync(string userId, UpdateProfileDto dto)
        {
            Log.Information("Розпочато оновлення профілю користувача з ID {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                Log.Warning("Користувача з ID {UserId} не знайдено для оновлення", userId);

                return "Користувача з таким ідентифікатором не знайдено.";
            }

            user.UserName = dto.UserName ?? user.UserName;
            user.Email = dto.Email ?? user.Email;
            user.ProfilePicture = dto.ProfilePicture ?? user.ProfilePicture;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            user.Address = dto.Address ?? user.Address;

            await _userRepository.UpdateAsync(user);

            Log.Information("Профіль користувача {UserId} успішно оновлено", userId);

            return user;
        }
    }
}