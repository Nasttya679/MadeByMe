using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Infrastructure.Repositories.Implementations;
using System.Linq;

namespace MadeByMe.Application.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IBuyerCartRepository _buyerCartRepo;
        private readonly IPostRepository _postRepo;

        public CartService(
            ICartRepository cartRepo,
            IBuyerCartRepository buyerCartRepo,
            IPostRepository postRepo)
        {
            _cartRepo = cartRepo;
            _buyerCartRepo = buyerCartRepo;
            _postRepo = postRepo;
        }

        public Cart? GetUserCartEntity(string buyerId)
        {
            return _cartRepo.GetCartByBuyerId(buyerId);
        }

        public CartViewModel GetUserCart(string buyerId)
        {
            var cart = _cartRepo.GetCartByBuyerId(buyerId);
            if (cart == null || cart.BuyerCarts == null || !cart.BuyerCarts.Any())
            {
                return new CartViewModel
                {
                    Items = new System.Collections.Generic.List<CartItem>(),
                    TotalPrice = 0
                };
            }

            var items = cart.BuyerCarts.Select(bc =>
            {
                var post = _postRepo.GetById(bc.PostId);
                return new CartItem
                {
                    Id = bc.CartItemId,
                    ProductId = bc.PostId,
                    Quantity = bc.Quantity,
                    Product = new Product
                    {
                        Id = post.Id,
                        Name = post.Title,
                        Price = post.Price,
                        ImageUrl = post.Photos.FirstOrDefault()?.FilePath ?? "/images/default.jpg",
                        Seller = post.Seller
                    }
                };
            }).ToList();

            var total = items.Sum(i => i.Product.Price * i.Quantity);

            return new CartViewModel
            {
                Items = items,
                TotalPrice = total
            };
        }

        public decimal GetCartTotal(int cartId)
        {
            var items = _buyerCartRepo.GetItemsByCartId(cartId);
            if (items == null || !items.Any()) return 0;

            return items.Sum(bc => bc.Quantity * _postRepo.GetById(bc.PostId).Price);
        }

        public void ClearCart(int cartId)
        {
            var items = _buyerCartRepo.GetItemsByCartId(cartId);
            _buyerCartRepo.RemoveRange(items);
        }

        public void UpdateCartItem(BuyerCart cartItem)
        {
            _buyerCartRepo.UpdateItem(cartItem);
        }
    }
}