using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Result<Order>> CreateOrderAsync(string buyerId, OrderDto dto);

        Task<Result<List<Order>>> GetBuyerHistoryAsync(string buyerId);

        Task<Result<List<Order>>> GetSellerJournalAsync(string sellerId);

        Task<Result> UpdateOrderStatusAsync(int orderId, string status, string currentUserId);

        Task<Result<IEnumerable<SellerOrderDto>>> GetSellerOrdersAsync(string sellerId);
    }
}