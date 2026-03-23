using MadeByMe.Application.DTOs;


namespace MadeByMe.Application.Services.Interfaces
{
    public interface IBuyerCartService
    {
        bool AddToCart(string userId, AddToCartDto addToCartDto);
        bool RemoveFromCart(string buyerId, int postId);
    }
}