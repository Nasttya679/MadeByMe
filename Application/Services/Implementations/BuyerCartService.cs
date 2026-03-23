using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Application.Services.Interfaces;

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

        public bool AddToCart(string userId, AddToCartDto addToCartDto)
        {
            // Перевірка існування поста
            var post = _postRepo.GetById(addToCartDto.PostId);
            if (post == null) throw new KeyNotFoundException("Товар не знайдено.");

            // Отримати або створити кошик користувача
            var cart = _cartRepo.GetCartByBuyerId(userId);
            if (cart == null)
            {
                cart = new Cart { BuyerId = userId };
                _cartRepo.AddCart(cart);
            }

            // Перевірка, чи товар уже в кошику
            var existingItem = _buyerCartRepo.GetItem(cart.CartId, addToCartDto.PostId);
            if (existingItem != null)
            {
                existingItem.Quantity += addToCartDto.Quantity;
                _buyerCartRepo.UpdateItem(existingItem);
            }
            else
            {
                var cartItem = new BuyerCart
                {
                    CartId = cart.CartId,
                    PostId = addToCartDto.PostId,
                    Quantity = addToCartDto.Quantity
                };
                _buyerCartRepo.AddItem(cartItem);
            }

            return true;
        }

        public bool RemoveFromCart(string buyerId, int postId)
        {
            var cart = _cartRepo.GetCartByBuyerId(buyerId);
            if (cart == null) return false;

            var item = _buyerCartRepo.GetItem(cart.CartId, postId);
            if (item != null)
            {
                _buyerCartRepo.RemoveItem(item);
                return true;
            }

            return false;
        }
    }
}