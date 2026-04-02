using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByBuyerIdAsync(string buyerId);

        Task AddCartAsync(Cart cart);

        Task UpdateCartAsync(Cart cart);

        Task RemoveCartAsync(Cart cart);
    }
}