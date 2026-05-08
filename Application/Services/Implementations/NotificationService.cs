namespace MadeByMe.Application.Services.Implementations;

using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Application.DTOs;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Domain.Entities;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo) => _repo = repo;

    public async Task CreateNotificationAsync(string userId, string message, string? actionUrl = null)
    {
        await _repo.AddAsync(new Notification
        {
            UserId = userId,
            Message = message,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow,
        });
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId)
    {
        var list = await _repo.GetUserNotificationsAsync(userId);
        return list.Select(n => new NotificationDto
        {
            Id = n.Id,
            Message = n.Message,
            ActionUrl = n.ActionUrl,
            CreatedAt = n.CreatedAt,
            IsRead = n.IsRead,
        }).ToList();
    }

    public async Task CreateNotificationAsync(string userId, string message, string? actionUrl = null, string? senderAvatar = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            ActionUrl = actionUrl,
            SenderAvatar = senderAvatar,
            CreatedAt = DateTime.UtcNow,
            IsSent = false,
            IsRead = false,
        };
        await _repo.AddAsync(notification);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var notifications = await _repo.GetUserNotificationsAsync(userId);
        return notifications.Count(n => !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _repo.MarkAsReadAsync(notificationId);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _repo.GetUserNotificationsAsync(userId);

        foreach (var n in notifications.Where(x => !x.IsRead))
        {
            await _repo.MarkAsReadAsync(n.Id);
        }
    }
}