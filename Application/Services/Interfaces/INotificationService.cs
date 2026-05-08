using System.Collections.Generic;
using System.Threading.Tasks;
using MadeByMe.Application.DTOs;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message, string? actionUrl = null, string? senderAvatar = null);

        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);

        Task<int> GetUnreadCountAsync(string userId);

        Task MarkAsReadAsync(int notificationId);

        Task MarkAllAsReadAsync(string userId);
    }
}