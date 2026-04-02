using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);

        Task<Order?> GetOrderByIdAsync(int id);

        Task<List<Order>> GetOrdersByBuyerIdAsync(string buyerId);

        Task<List<Order>> GetOrdersBySellerIdAsync(string sellerId);

        Task UpdateOrderStatusAsync(int orderId, string status);
    }
}