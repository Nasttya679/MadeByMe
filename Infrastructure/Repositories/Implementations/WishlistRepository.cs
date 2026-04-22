using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Infrastructure.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Wishlist?> GetWishlistItemAsync(string userId, int postId)
        {
            return await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.PostId == postId);
        }

        public async Task<List<Wishlist>> GetUserWishlistAsync(string userId)
        {
            return await _context.Wishlists
                .Include(w => w.Post)
                .ThenInclude(p => p!.Photos)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Wishlist item)
        {
            await _context.Wishlists.AddAsync(item);
        }

        public void Remove(Wishlist item)
        {
            _context.Wishlists.Remove(item);
        }

        public async Task<int> GetCountAsync(string userId)
        {
            return await _context.Wishlists.CountAsync(w => w.UserId == userId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}