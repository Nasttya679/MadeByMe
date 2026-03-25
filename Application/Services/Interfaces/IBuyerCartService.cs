using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IBuyerCartService
    {
        // Змінили bool на Result
        Result AddToCart(string userId, AddToCartDto addToCartDto);

        Result RemoveFromCart(string buyerId, int postId);
    }
}