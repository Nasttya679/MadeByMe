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

            var cartResult = _cartService.GetUserCartEntity(buyerId);
            if (cartResult.IsFailure || cartResult.Value == null || !cartResult.Value.BuyerCarts.Any())
            {
                return Result<Order>.Failure("Ваш кошик порожній.");
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
            };

            decimal total = 0;
            foreach (var cartItem in cart.BuyerCarts)
            {
                var orderItem = new OrderItem
                {
                    PostId = cartItem.PostId,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = cartItem.Post!.Price,
                };

                order.OrderItems.Add(orderItem);
                total += cartItem.Quantity * cartItem.Post.Price;
            }

            order.TotalAmount = total;

            await _orderRepo.CreateOrderAsync(order);

            _cartService.ClearCart(cart.CartId);

            Log.Information("Замовлення {OrderId} успішно створено", order.Id);
            return Result<Order>.Success(order);
        }

        public async Task<Result<List<Order>>> GetBuyerHistoryAsync(string buyerId)
        {
            var orders = await _orderRepo.GetOrdersByBuyerIdAsync(buyerId);
            return Result<List<Order>>.Success(orders);
        }

        public async Task<Result<List<Order>>> GetSellerJournalAsync(string sellerId)
        {
            var orders = await _orderRepo.GetOrdersBySellerIdAsync(sellerId);
            return Result<List<Order>>.Success(orders);
        }

        public async Task<Result> UpdateOrderStatusAsync(int orderId, string status, string currentUserId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return Result.Failure("Замовлення не знайдено.");
            }

            bool isSeller = order.OrderItems.Any(oi => oi.Post!.SellerId == currentUserId);
            if (!isSeller)
            {
                return Result.Failure("У вас немає прав змінювати статус цього замовлення.");
            }

            await _orderRepo.UpdateOrderStatusAsync(orderId, status);
            return Result.Success();
        }
    }
}