using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;

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
            var post = _postRepo.GetById(addToCartDto.PostId);
            if (post == null)
            {
                return Result.Failure("Товар не знайдено.");
            }

            var cart = _cartRepo.GetCartByBuyerId(userId);
            if (cart == null)
            {
                cart = new Cart { BuyerId = userId };
                _cartRepo.AddCart(cart);
            }

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
                    Quantity = addToCartDto.Quantity,
                };
                _buyerCartRepo.AddItem(cartItem);
            }

            return Result.Success();
        }

        public Result RemoveFromCart(string buyerId, int postId)
        {
            var cart = _cartRepo.GetCartByBuyerId(buyerId);

            if (cart == null)
            {
                return Result.Failure("Кошик користувача не знайдено.");
            }

            var item = _buyerCartRepo.GetItem(cart.CartId, postId);

            if (item == null)
            {
                return Result.Failure("Товар не знайдено у кошику.");
            }

            _buyerCartRepo.RemoveItem(item);
            return Result.Success();
        }
    }
}