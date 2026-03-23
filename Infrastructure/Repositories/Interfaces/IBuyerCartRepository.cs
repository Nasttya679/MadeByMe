
using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IBuyerCartRepository
    {
        BuyerCart? GetItem(int cartId, int postId);
        void AddItem(BuyerCart item);
        void RemoveItem(BuyerCart item);
        void UpdateItem(BuyerCart item);
        void RemoveRange(IEnumerable<BuyerCart> items);

        IEnumerable<BuyerCart> GetItemsByCartId(int cartId);
    }
}