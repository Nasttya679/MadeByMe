using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class BuyerCartRepository : IBuyerCartRepository
    {
        private readonly ApplicationDbContext _context;

        public BuyerCartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BuyerCart?> GetItemAsync(int cartId, int postId)
        {
            return await _context.BuyerCarts.FirstOrDefaultAsync(bc => bc.CartId == cartId && bc.PostId == postId);
        }

        public async Task AddItemAsync(BuyerCart item)
        {
            _context.BuyerCarts.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(BuyerCart item)
        {
            _context.BuyerCarts.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(BuyerCart item)
        {
            _context.BuyerCarts.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<BuyerCart> items)
        {
            _context.BuyerCarts.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BuyerCart>> GetItemsByCartIdAsync(int cartId)
        {
            return await _context.BuyerCarts
                .Where(bc => bc.CartId == cartId)
                .ToListAsync();
        }
    }
}