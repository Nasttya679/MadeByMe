using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IBuyerCartRepository
    {
        Task<BuyerCart?> GetItemAsync(int cartId, int postId);

        Task AddItemAsync(BuyerCart item);

        Task RemoveItemAsync(BuyerCart item);

        Task UpdateItemAsync(BuyerCart item);

        Task RemoveRangeAsync(IEnumerable<BuyerCart> items);

        Task<IEnumerable<BuyerCart>> GetItemsByCartIdAsync(int cartId);
    }
}