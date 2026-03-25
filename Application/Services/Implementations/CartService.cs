using System.Collections.Generic;
using System.Linq;
using MadeByMe.Application.Common;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;

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

        public Result<Cart> GetUserCartEntity(string buyerId)
        {
            var cart = _cartRepo.GetCartByBuyerId(buyerId);
            if (cart == null)
            {
                return Result<Cart>.Failure("Кошик користувача не знайдено.");
            }

            return Result<Cart>.Success(cart);
        }

        public Result<CartViewModel> GetUserCart(string buyerId)
        {
            var cart = _cartRepo.GetCartByBuyerId(buyerId);
            if (cart == null || cart.BuyerCarts == null || !cart.BuyerCarts.Any())
            {
                // Порожній кошик - це не помилка, це просто успішний порожній результат
                return Result<CartViewModel>.Success(new CartViewModel
                {
                    Items = new List<CartItem>(),
                    TotalPrice = 0,
                });
            }

            var items = new List<CartItem>();
            foreach (var bc in cart.BuyerCarts)
            {
                var post = _postRepo.GetById(bc.PostId);

                // Усуваємо ризик NullReferenceException!
                if (post == null)
                {
                    return Result<CartViewModel>.Failure($"Товар з ID {bc.PostId} більше не існує.");
                }

                items.Add(new CartItem
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
                        Seller = post.Seller,
                    },
                });
            }

            var total = items.Sum(i => i.Product!.Price * i.Quantity);

            return Result<CartViewModel>.Success(new CartViewModel
            {
                Items = items,
                TotalPrice = total,
            });
        }

        public Result<decimal> GetCartTotal(int cartId)
        {
            var items = _buyerCartRepo.GetItemsByCartId(cartId);
            if (items == null || !items.Any())
            {
                return Result<decimal>.Success(0);
            }

            decimal total = 0;
            foreach (var bc in items)
            {
                var post = _postRepo.GetById(bc.PostId);
                if (post == null)
                {
                    // Знову ж таки, захищаємось від помилки бази даних
                    return Result<decimal>.Failure($"Помилка розрахунку: товар з ID {bc.PostId} не знайдено.");
                }

                total += bc.Quantity * post.Price;
            }

            return Result<decimal>.Success(total);
        }

        public Result ClearCart(int cartId)
        {
            var items = _buyerCartRepo.GetItemsByCartId(cartId);
            if (items != null && items.Any())
            {
                _buyerCartRepo.RemoveRange(items);
            }

            return Result.Success();
        }

        public Result UpdateCartItem(BuyerCart cartItem)
        {
            if (cartItem == null)
            {
                return Result.Failure("Недійсні дані для оновлення кошика.");
            }

            _buyerCartRepo.UpdateItem(cartItem);
            return Result.Success();
        }
    }
}