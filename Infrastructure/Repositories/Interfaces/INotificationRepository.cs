using System.Collections.Generic;
using System.Threading.Tasks;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);

        Task<List<Notification>> GetUnsentNotificationsAsync();

        Task MarkAsSentAsync(IEnumerable<Notification> notifications);

        Task<List<Notification>> GetUserNotificationsAsync(string userId);

        Task MarkAsReadAsync(int notificationId);
    }
}