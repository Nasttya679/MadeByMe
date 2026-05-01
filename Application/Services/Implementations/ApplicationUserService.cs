using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
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

        public async Task<Result<ApplicationUser>> GetSellerProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "Користувача (продавця) не знайдено.";
            }

            return user;
        }

        public async Task<Result> UpdateSellerDescriptionAsync(string userId, string? description)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "Користувача не знайдено.";
            }

            user.SellerDescription = description;
            await _userManager.UpdateAsync(user);

            Log.Information("Продавець {UserId} оновив опис своєї вітрини", userId);

            return Result.Success();
        }
    }
}