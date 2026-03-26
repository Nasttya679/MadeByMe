using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class BuyerCartService : IBuyerCartService
    {
        private readonly IPostRepository _postRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IBuyerCartRepository _buyerCartRepo;

        public BuyerCartService(
            IPostRepository postRepo,
            ICartRepository cartRepo,
            IBuyerCartRepository buyerCartRepo)
        {
            _postRepo = postRepo;
            _cartRepo = cartRepo;
            _buyerCartRepo = buyerCartRepo;
        }

        public Result AddToCart(string userId, AddToCartDto addToCartDto)
        {
            Log.Information("Початок додавання товару {PostId} у кошик для користувача {UserId}", addToCartDto.PostId, userId);
            var post = _postRepo.GetById(addToCartDto.PostId);
            if (post == null)
            {
                Log.Warning("Товар {PostId} не знайдено при спробі додавання у кошик", addToCartDto.PostId);
                return Result.Failure("Товар не знайдено.");
            }

            var cart = _cartRepo.GetCartByBuyerId(userId);
            if (cart == null)
            {
                Log.Information("Кошик для користувача {UserId} не знайдено, створюється новий кошик", userId);
                cart = new Cart { BuyerId = userId };
                _cartRepo.AddCart(cart);
            }

            var existingItem = _buyerCartRepo.GetItem(cart.CartId, addToCartDto.PostId);
            if (existingItem != null)
            {
                existingItem.Quantity += addToCartDto.Quantity;
                _buyerCartRepo.UpdateItem(existingItem);
                Log.Information("Оновлено кількість товару {PostId} у кошику (ID кошика: {CartId})", addToCartDto.PostId, cart.CartId);
            }
            else
            {
                var cartItem = new BuyerCart
                {
                    CartId = cart.CartId,
                    PostId = addToCartDto.PostId,
                    Quantity = addToCartDto.Quantity,
                };
                _buyerCartRepo.AddItem(cartItem);
                Log.Information("Товар {PostId} успішно додано як новий елемент у кошик {CartId}", addToCartDto.PostId, cart.CartId);
            }

            return Result.Success();
        }

        public Result RemoveFromCart(string buyerId, int postId)
        {
            Log.Information("Спроба видалення товару {PostId} з кошика користувача {UserId}", postId, buyerId);
            var cart = _cartRepo.GetCartByBuyerId(buyerId);

            if (cart == null)
            {
                Log.Warning("Кошик для користувача {UserId} не знайдено при спробі видалення товару", buyerId);
                return Result.Failure("Кошик користувача не знайдено.");
            }

            var item = _buyerCartRepo.GetItem(cart.CartId, postId);

            if (item == null)
            {
                Log.Warning("Товар {PostId} не знайдено у кошику {CartId}", postId, cart.CartId);
                return Result.Failure("Товар не знайдено у кошику.");
            }

            _buyerCartRepo.RemoveItem(item);
            Log.Information("Товар {PostId} успішно видалено з кошика {CartId} для користувача {UserId}", postId, cart.CartId, buyerId);
            return Result.Success();
        }
    }
}