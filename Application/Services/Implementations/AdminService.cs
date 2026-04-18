using MadeByMe.Application.Common;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;

        public AdminService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<List<ApplicationUser>>> GetUsersAsync()
        {
            Log.Information("Отримання списку всіх користувачів (окрім адміністраторів)");
            var users = await _userRepository.GetAllExceptAdminsAsync();

            return users;
        }

        public async Task<Result> BlockUserAsync(string userId)
        {
            Log.Information("Спроба блокування користувача з ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                Log.Warning("Невдала спроба блокування: користувача з ID {UserId} не знайдено", userId);
                return "Користувача не знайдено.";
            }

            user.IsBlocked = true;
            await _userRepository.UpdateAsync(user);

            Log.Information("Користувача {UserId} успішно заблоковано", userId);
            return Result.Success();
        }

        public async Task<Result> UnblockUserAsync(string userId)
        {
            Log.Information("Спроба розблокування користувача з ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                Log.Warning("Невдала спроба розблокування: користувача з ID {UserId} не знайдено", userId);
                return "Користувача не знайдено.";
            }

            user.IsBlocked = false;
            await _userRepository.UpdateAsync(user);

            Log.Information("Користувача {UserId} успішно розблоковано", userId);
            return Result.Success();
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            Log.Information("Спроба видалення користувача з ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                Log.Warning("Невдала спроба видалення: користувача з ID {UserId} не знайдено", userId);
                return "Користувача не знайдено.";
            }

            await _userRepository.DeleteAsync(user);

            Log.Information("Користувача {UserId} успішно видалено", userId);
            return Result.Success();
        }
    }
}