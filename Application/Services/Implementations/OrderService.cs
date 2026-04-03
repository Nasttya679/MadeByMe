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
                Status = "Paid",
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

        public async Task<Result<List<Order>>> GetBuyerHistoryAsync(string buyerId)
        {
            var orders = await _orderRepo.GetOrdersByBuyerIdAsync(buyerId);
            return orders;
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
                return "Замовлення не знайдено.";
            }

            bool isSeller = order.OrderItems.Any(oi => oi.Post != null && oi.Post.SellerId == currentUserId);
            if (!isSeller)
            {
                return "У вас немає прав змінювати статус цього замовлення.";
            }

            await _orderRepo.UpdateOrderStatusAsync(orderId, status);
            return Result.Success();
        }

        public async Task<Result<List<Order>>> GetUserOrderHistoryAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Result<List<Order>>.Failure("Ідентифікатор користувача не знайдено.");
            }

            var orders = await _orderRepo.GetOrdersByUserIdAsync(userId);

            return Result<List<Order>>.Success(orders.ToList());
        }
    }
}