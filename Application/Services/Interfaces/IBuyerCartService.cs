using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IBuyerCartService
    {
        Task<Result> AddToCartAsync(string userId, AddToCartDto addToCartDto);

        Task<Result> RemoveFromCartAsync(string buyerId, int postId);
    }
}