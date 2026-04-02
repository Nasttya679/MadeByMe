using MadeByMe.Application.Common;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface ICartService
    {
        Task<Result<Cart>> GetUserCartEntityAsync(string buyerId);

        Task<Result<CartViewModel>> GetUserCartAsync(string buyerId);

        Task<Result<decimal>> GetCartTotalAsync(int cartId);

        Task<Result> ClearCartAsync(int cartId);

        Task<Result> UpdateCartItemAsync(BuyerCart cartItem);
    }
}