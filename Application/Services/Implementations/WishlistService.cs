using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MadeByMe.Application.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _repository;
        private readonly ILogger<WishlistService> _logger;
        private readonly ProjectSettings _settings;

        public WishlistService(
            IWishlistRepository repository,
            ILogger<WishlistService> logger,
            IOptions<ProjectSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _settings = options.Value;
        }

        public async Task<Result<List<WishlistItemDto>>> GetUserWishlistAsync(string userId)
        {
            _logger.LogInformation("Отримання списку обраного для користувача {UserId}", userId);

            var items = await _repository.GetUserWishlistAsync(userId);

            var dtos = items.Select(w => new WishlistItemDto
            {
                PostId = w.PostId,
                Title = w.Post!.Title ?? "Без назви",
                Price = w.Post!.Price,
                PhotoUrl = w.Post!.Photos.FirstOrDefault() != null
                    ? w.Post.Photos.FirstOrDefault()!.FilePath!
                    : _settings.FileStorage.DefaultImagePath,
                AddedAt = w.AddedAt,
            }).ToList();

            _logger.LogInformation("Знайдено {Count} товарів у списку обраного для користувача {UserId}", dtos.Count, userId);

            return dtos;
        }

        public async Task<Result<(bool IsAdded, int TotalCount)>> ToggleFavoriteAsync(string userId, int postId)
        {
            _logger.LogInformation("Запит на зміну статусу обраного для товару {PostId} від користувача {UserId}", postId, userId);

            var existingItem = await _repository.GetWishlistItemAsync(userId, postId);
            bool isAdded;

            if (existingItem != null)
            {
                _repository.Remove(existingItem);
                isAdded = false;
                _logger.LogInformation("Товар {PostId} успішно видалено з обраного користувача {UserId}", postId, userId);
            }
            else
            {
                var newItem = new Wishlist { UserId = userId, PostId = postId };
                await _repository.AddAsync(newItem);
                isAdded = true;
                _logger.LogInformation("Товар {PostId} успішно додано до обраного користувача {UserId}", postId, userId);
            }

            await _repository.SaveChangesAsync();

            var totalCount = await _repository.GetCountAsync(userId);

            return (isAdded, totalCount);
        }

        public async Task<Result<int>> GetWishlistCountAsync(string userId)
        {
            var count = await _repository.GetCountAsync(userId);
            return count;
        }
    }
}