using MadeByMe.Application.Common;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface ICartService
    {
        Result<Cart> GetUserCartEntity(string buyerId);

        Result<CartViewModel> GetUserCart(string buyerId);

        Result<decimal> GetCartTotal(int cartId);

        Result ClearCart(int cartId);

        Result UpdateCartItem(BuyerCart cartItem);
    }
}