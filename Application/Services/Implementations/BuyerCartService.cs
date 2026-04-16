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

        public async Task<Result> AddToCartAsync(string userId, AddToCartDto addToCartDto)
        {
            Log.Information("Початок додавання товару {PostId} у кошик для користувача {UserId}", addToCartDto.PostId, userId);

            var post = await _postRepo.GetByIdAsync(addToCartDto.PostId);
            if (post == null)
            {
                Log.Warning("Товар {PostId} не знайдено при спробі додавання у кошик", addToCartDto.PostId);
                return "Товар не знайдено.";
            }

            var cart = await _cartRepo.GetCartByBuyerIdAsync(userId);
            if (cart == null)
            {
                Log.Information("Кошик для користувача {UserId} не знайдено, створюється новий кошик", userId);
                cart = new Cart { BuyerId = userId };
                await _cartRepo.AddCartAsync(cart);
            }

            var existingItem = await _buyerCartRepo.GetItemAsync(cart.CartId, addToCartDto.PostId);
            if (existingItem != null)
            {
                existingItem.Quantity += addToCartDto.Quantity;
                await _buyerCartRepo.UpdateItemAsync(existingItem);
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
                await _buyerCartRepo.AddItemAsync(cartItem);
                Log.Information("Товар {PostId} успішно додано як новий елемент у кошик {CartId}", addToCartDto.PostId, cart.CartId);
            }

            return Result.Success();
        }

        public async Task<Result> RemoveFromCartAsync(string buyerId, int postId)
        {
            Log.Information("Спроба видалення товару {PostId} з кошика користувача {UserId}", postId, buyerId);

            var cart = await _cartRepo.GetCartByBuyerIdAsync(buyerId);

            if (cart == null)
            {
                Log.Warning("Кошик для користувача {UserId} не знайдено при спробі видалення товару", buyerId);
                return "Кошик користувача не знайдено.";
            }

            var item = await _buyerCartRepo.GetItemAsync(cart.CartId, postId);

            if (item == null)
            {
                Log.Warning("Товар {PostId} не знайдено у кошику {CartId}", postId, cart.CartId);
                return "Товар не знайдено у кошику.";
            }

            await _buyerCartRepo.RemoveItemAsync(item);
            Log.Information("Товар {PostId} успішно видалено з кошика {CartId} для користувача {UserId}", postId, cart.CartId, buyerId);

            return Result.Success();
        }
    }
}