
using MadeByMe.Domain.Entities;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.ViewModels;


namespace MadeByMe.Application.Services.Interfaces {

    public interface ICartService
    {
        Cart GetUserCartEntity(string buyerId);
        CartViewModel GetUserCart(string buyerId);
        decimal GetCartTotal(int cartId);
        void ClearCart(int cartId);
        void UpdateCartItem(BuyerCart cartItem);
    }
}
