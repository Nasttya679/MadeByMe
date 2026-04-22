using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories
{
    public interface IWishlistRepository
    {
        Task<Wishlist?> GetWishlistItemAsync(string userId, int postId);

        Task<List<Wishlist>> GetUserWishlistAsync(string userId);

        Task AddAsync(Wishlist item);

        void Remove(Wishlist item);

        Task<int> GetCountAsync(string userId);

        Task SaveChangesAsync();
    }
}