using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByBuyerIdAsync(string buyerId)
        {
            return await _context.Carts
                .Include(c => c.BuyerCarts)
                .ThenInclude(bc => bc.Post)
                .FirstOrDefaultAsync(c => c.BuyerId == buyerId);
        }

        public async Task AddCartAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartAsync(Cart cart)
        {
            _context.Carts.Remove(cart);
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