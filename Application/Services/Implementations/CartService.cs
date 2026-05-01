using MadeByMe.Application.Common;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.ViewModels;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IBuyerCartRepository _buyerCartRepo;
        private readonly IPostRepository _postRepo;
        private readonly ProjectSettings _settings;
        private readonly IExchangeRateService _exchangeRateService;

        public CartService(
            ICartRepository cartRepo,
            IBuyerCartRepository buyerCartRepo,
            IPostRepository postRepo,
            IOptions<ProjectSettings> options,
            IExchangeRateService exchangeRateService)
        {
            _cartRepo = cartRepo;
            _buyerCartRepo = buyerCartRepo;
            _postRepo = postRepo;
            _settings = options.Value;
            _exchangeRateService = exchangeRateService;
        }

        public async Task<Result<Cart>> GetUserCartEntityAsync(string buyerId)
        {
            var cart = await _cartRepo.GetCartByBuyerIdAsync(buyerId);
            if (cart == null)
            {
                Log.Warning("Кошик для покупця {BuyerId} не знайдено в репозиторії", buyerId);
                return "Кошик користувача не знайдено.";
            }

            return cart;
        }

        public async Task<Result<CartViewModel>> GetUserCartAsync(string buyerId)
        {
            Log.Information("Отримання детального вмісту кошика для покупця {BuyerId}", buyerId);
            var cart = await _cartRepo.GetCartByBuyerIdAsync(buyerId);

            if (cart == null || cart.BuyerCarts == null || !cart.BuyerCarts.Any())
            {
                return new CartViewModel
                {
                    Items = new List<CartItem>(),
                    TotalPrice = 0,
                    TotalPriceUsd = 0,
                };
            }

            var items = new List<CartItem>();
            foreach (var bc in cart.BuyerCarts)
            {
                var post = await _postRepo.GetByIdAsync(bc.PostId);

                if (post == null)
                {
                    Log.Error("Помилка цілісності даних: товар {PostId} відсутній", bc.PostId);
                    return $"Товар з ID {bc.PostId} більше не існує.";
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
                        ImageUrl = post.Photos.FirstOrDefault()?.FilePath ?? _settings.FileStorage.DefaultImagePath,
                        Seller = post.Seller,
                    },
                });
            }

            var total = items.Sum(i => i.Product!.Price * i.Quantity);

            var usdRate = await _exchangeRateService.GetUsdRateAsync();
            var totalUsd = Math.Round(total / usdRate, 2);

            Log.Information("Кошик завантажений. Сума: {Total} грн (~{TotalUsd} USD)", total, totalUsd);

            return new CartViewModel
            {
                Items = items,
                TotalPrice = total,
                TotalPriceUsd = totalUsd,
                UsdRate = usdRate,
            };
        }

        public async Task<Result<decimal>> GetCartTotalAsync(int cartId)
        {
            var items = await _buyerCartRepo.GetItemsByCartIdAsync(cartId);
            if (items == null || !items.Any())
            {
                return 0m;
            }

            decimal total = 0;
            foreach (var bc in items)
            {
                var post = await _postRepo.GetByIdAsync(bc.PostId);
                if (post == null)
                {
                    Log.Warning("Неможливо розрахувати суму кошика {CartId}: товар {PostId} не знайдено", cartId, bc.PostId);
                    return $"Помилка розрахунку: товар з ID {bc.PostId} не знайдено.";
                }

                total += bc.Quantity * post.Price;
            }

            return total;
        }

        public async Task<Result> ClearCartAsync(int cartId)
        {
            Log.Information("Розпочато процес повного очищення кошика з ID {CartId}", cartId);
            var items = await _buyerCartRepo.GetItemsByCartIdAsync(cartId);
            if (items != null && items.Any())
            {
                await _buyerCartRepo.RemoveRangeAsync(items);
                Log.Information("Кошик {CartId} успішно очищено. Видалено позицій: {Count}", cartId, items.Count());
            }

            return Result.Success();
        }

        public async Task<Result> UpdateCartItemAsync(BuyerCart cartItem)
        {
            if (cartItem == null)
            {
                Log.Warning("Спроба оновлення елемента кошика з порожніми даними (null)");
                return "Недійсні дані для оновлення кошика.";
            }

            Log.Information("Оновлення параметрів товару {PostId} у кошику {CartId}. Нова кількість: {Quantity}", cartItem.PostId, cartItem.CartId, cartItem.Quantity);

            await _buyerCartRepo.UpdateItemAsync(cartItem);
            return Result.Success();
        }
    }
}