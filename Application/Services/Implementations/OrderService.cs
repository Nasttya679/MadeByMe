using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartService _cartService;

        public OrderService(IOrderRepository orderRepo, ICartService cartService)
        {
            _orderRepo = orderRepo;
            _cartService = cartService;
        }

        public async Task<Result<Order>> CreateOrderAsync(string buyerId, OrderDto dto)
        {
            Log.Information("Початок оформлення замовлення для покупця {BuyerId}", buyerId);

            var cartResult = await _cartService.GetUserCartEntityAsync(buyerId);
            if (cartResult.IsFailure || cartResult.Value == null || !cartResult.Value.BuyerCarts.Any())
            {
                Log.Warning("Помилка створення замовлення: кошик покупця {BuyerId} порожній", buyerId);
                return "Ваш кошик порожній.";
            }

            var cart = cartResult.Value;

            var order = new Order
            {
                BuyerId = buyerId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                City = dto.City,
                PostOffice = dto.PostOffice,
                Status = "Processing",
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 0,
                OrderItems = new List<OrderItem>(),
            };

            decimal total = 0;
            foreach (var cartItem in cart.BuyerCarts)
            {
                if (cartItem.Post == null)
                {
                    continue;
                }

                var orderItem = new OrderItem
                {
                    PostId = cartItem.PostId,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = cartItem.Post.Price,
                };

                order.OrderItems.Add(orderItem);
                total += cartItem.Quantity * cartItem.Post.Price;
            }

            order.TotalAmount = total;
            await _orderRepo.CreateOrderAsync(order);
            await _cartService.ClearCartAsync(cart.CartId);

            Log.Information("Замовлення {OrderId} успішно створено", order.Id);
            return order;
        }

        public async Task<Result<List<Order>>> GetBuyerHistoryAsync(string buyerId, string? status = "All", string? search = null, DateTime? date = null)
        {
            var orders = await _orderRepo.GetOrdersByBuyerIdAsync(buyerId);

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                orders = orders.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                orders = orders.Where(o => o.OrderItems.Any(oi =>
                    oi.Post != null && oi.Post.Title!.ToLower().Contains(search))).ToList();
            }

            if (date.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt.Date == date.Value.Date).ToList();
            }

            return orders.OrderByDescending(o => o.CreatedAt).ToList();
        }

        public async Task<Result<List<Order>>> GetSellerJournalAsync(string sellerId)
        {
            var orders = await _orderRepo.GetOrdersBySellerIdAsync(sellerId);
            return orders;
        }

        public async Task<Result> UpdateOrderStatusAsync(int orderId, string status, string currentUserId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                Log.Warning("Замовлення {OrderId} не знайдено при спробі оновити статус", orderId);
                return "Замовлення не знайдено.";
            }

            bool isSeller = order.OrderItems.Any(oi => oi.Post != null && oi.Post.SellerId == currentUserId);
            if (!isSeller)
            {
                Log.Warning("Користувач {UserId} намагався змінити статус чужого замовлення {OrderId}", currentUserId, orderId);
                return "У вас немає прав змінювати статус цього замовлення.";
            }

            await _orderRepo.UpdateOrderStatusAsync(orderId, status);
            Log.Information("Статус замовлення {OrderId} змінено на {Status} продавцем {UserId}", orderId, status, currentUserId);

            return Result.Success();
        }

        public async Task<Result<IEnumerable<SellerOrderDto>>> GetSellerOrdersAsync(string sellerId, string? status = "All", string? search = null, DateTime? date = null)
        {
            var orders = await _orderRepo.GetOrdersBySellerIdAsync(sellerId);

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                orders = orders.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                orders = orders.Where(o => o.OrderItems.Any(oi =>
                    oi.Post != null && oi.Post.SellerId == sellerId && oi.Post.Title!.ToLower().Contains(search))).ToList();
            }

            if (date.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt.Date == date.Value.Date).ToList();
            }

            var sellerOrders = orders.Select(o => new SellerOrderDto
            {
                OrderId = o.Id,
                BuyerName = $"{o.FirstName} {o.LastName}",
                BuyerEmail = o.Email ?? string.Empty,
                BuyerPhone = o.PhoneNumber ?? "Не вказано",
                ShippingAddress = $"{o.City}, {o.PostOffice}",
                OrderDate = o.CreatedAt,
                Status = o.Status,
                ReturnReason = o.ReturnReason,
                CancellationReason = o.CancellationReason,
                TotalPrice = o.OrderItems
                    .Where(oi => oi.Post != null && oi.Post.SellerId == sellerId)
                    .Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                Items = o.OrderItems
                    .Where(oi => oi.Post != null && oi.Post.SellerId == sellerId)
                    .Select(oi => new SellerOrderItemDto
                    {
                        ProductName = oi.Post?.Title ?? "Товар",
                        Quantity = oi.Quantity,
                        Price = oi.PriceAtPurchase,
                    }).ToList(),
            });

            return sellerOrders.ToList();
        }

        public async Task<Result<Order>> GetOrderByIdAndBuyerAsync(int orderId, string buyerId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);

            if (order == null || order.BuyerId != buyerId)
            {
                return "Замовлення не знайдено або у вас немає доступу.";
            }

            return order;
        }

        public async Task<Result> CancelOrderAsync(int orderId, string buyerId, string? reason)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);

            if (order == null || order.BuyerId != buyerId)
            {
                return "Замовлення не знайдено.";
            }

            if (order.Status != "Processing")
            {
                return "Скасувати можна лише замовлення, які ще знаходяться в обробці.";
            }

            order.Status = "Cancelled";
            order.CancellationReason = reason;

            await _orderRepo.UpdateOrderStatusAsync(orderId, "Cancelled");

            Log.Information("Покупець {BuyerId} скасував замовлення {OrderId}. Причина: {Reason}", buyerId, orderId, reason);

            return Result.Success();
        }

        public async Task<Result> RequestReturnAsync(int orderId, string buyerId, string reason)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);

            if (order == null || order.BuyerId != buyerId)
            {
                return "Замовлення не знайдено.";
            }

            if (order.Status != "Delivered")
            {
                return "Повернути можна лише ті замовлення, які вже доставлені.";
            }

            order.Status = "ReturnRequested";
            order.ReturnReason = reason;

            await _orderRepo.UpdateOrderStatusAsync(orderId, "ReturnRequested");

            Log.Information("Покупець {BuyerId} оформив запит на повернення замовлення {OrderId}. Причина: {Reason}", buyerId, orderId, reason);

            return Result.Success();
        }

        public async Task<Result> ApproveReturnAsync(int orderId, string sellerId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return "Замовлення не знайдено.";
            }

            bool isSeller = order.OrderItems.Any(oi => oi.Post != null && oi.Post.SellerId == sellerId);
            if (!isSeller)
            {
                return "У вас немає прав змінювати статус цього замовлення.";
            }

            if (order.Status != "ReturnRequested")
            {
                return "Це замовлення не очікує на повернення.";
            }

            order.Status = "Refunded";
            await _orderRepo.UpdateOrderStatusAsync(orderId, "Refunded");

            Log.Information("Продавець {SellerId} схвалив повернення коштів за замовлення {OrderId}", sellerId, orderId);

            return Result.Success();
        }
    }
}