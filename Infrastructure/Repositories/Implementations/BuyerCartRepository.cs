using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;


namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class BuyerCartRepository : IBuyerCartRepository
    {
        private readonly ApplicationDbContext _context;

        public BuyerCartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public BuyerCart? GetItem(int cartId, int postId)
        {
            return _context.BuyerCarts.FirstOrDefault(bc => bc.CartId == cartId && bc.PostId == postId);
        }

        public void AddItem(BuyerCart item)
        {
            _context.BuyerCarts.Add(item);
            _context.SaveChanges();
        }

        public void RemoveItem(BuyerCart item)
        {
            _context.BuyerCarts.Remove(item);
            _context.SaveChanges();
        }

        public void UpdateItem(BuyerCart item)
        {
            _context.BuyerCarts.Update(item);
            _context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<BuyerCart> items)
        {
            _context.BuyerCarts.RemoveRange(items);
            _context.SaveChanges();
        }

        // ----> Реалізація методу GetItemsByCartId
        public IEnumerable<BuyerCart> GetItemsByCartId(int cartId)
        {
            return _context.BuyerCarts
                .Where(bc => bc.CartId == cartId)
                .ToList();
        }
    }
}