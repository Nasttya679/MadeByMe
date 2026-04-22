using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<Result<List<WishlistItemDto>>> GetUserWishlistAsync(string userId);

        Task<Result<(bool IsAdded, int TotalCount)>> ToggleFavoriteAsync(string userId, int postId);

        Task<Result<int>> GetWishlistCountAsync(string userId);
    }
}