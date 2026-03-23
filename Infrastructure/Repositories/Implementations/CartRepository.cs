using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Infrastructure.Data;
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

        public Cart? GetCartByBuyerId(string buyerId)
        {
            return _context.Carts
                .Include(c => c.BuyerCarts)
                .ThenInclude(bc => bc.Post)
                .FirstOrDefault(c => c.BuyerId == buyerId);
        }

        public void AddCart(Cart cart)
        {
            _context.Carts.Add(cart);
            _context.SaveChanges();
        }

        public void UpdateCart(Cart cart)
        {
            _context.Carts.Update(cart);
            _context.SaveChanges();
        }

        public void RemoveCart(Cart cart)
        {
            _context.Carts.Remove(cart);
            _context.SaveChanges();
        }

        public IEnumerable<BuyerCart> GetItemsByCartId(int cartId)
        {
            return _context.BuyerCarts
                .Where(bc => bc.CartId == cartId)
                .ToList();
        }
    }
}