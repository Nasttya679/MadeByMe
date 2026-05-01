using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Result<Order>> CreateOrderAsync(string buyerId, OrderDto dto);

        Task<Result<List<Order>>> GetBuyerHistoryAsync(string buyerId, string? status = "All", string? search = null, DateTime? date = null);

        Task<Result<List<Order>>> GetSellerJournalAsync(string sellerId);

        Task<Result> UpdateOrderStatusAsync(int orderId, string status, string currentUserId);

        Task<Result<IEnumerable<SellerOrderDto>>> GetSellerOrdersAsync(string sellerId, string? status = "All", string? search = null, DateTime? date = null);

        Task<Result<Order>> GetOrderByIdAndBuyerAsync(int orderId, string buyerId);

        Task<Result> CancelOrderAsync(int orderId, string buyerId, string? reason);

        Task<Result> RequestReturnAsync(int orderId, string buyerId, string reason);

        Task<Result> ApproveReturnAsync(int orderId, string sellerId);
    }
}