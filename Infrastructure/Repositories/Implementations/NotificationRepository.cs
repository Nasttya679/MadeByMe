using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUnsentNotificationsAsync()
        {
            return await _context.Notifications
                .Where(n => !n.IsSent)
                .ToListAsync();
        }

        public async Task MarkAsSentAsync(IEnumerable<Notification> notifications)
        {
            foreach (var notification in notifications)
            {
                notification.IsSent = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}