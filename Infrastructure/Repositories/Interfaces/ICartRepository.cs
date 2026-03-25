using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Cart? GetCartByBuyerId(string buyerId);

        void AddCart(Cart cart);

        void UpdateCart(Cart cart);

        void RemoveCart(Cart cart);
    }
}