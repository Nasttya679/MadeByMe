using System.Threading.Tasks;
using MadeByMe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MadeByMe.Web.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var userId = CurrentUserId;

            if (userId == null)
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new { unreadCount = unreadCount, notifications = notifications });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (CurrentUserId != null)
            {
                await _notificationService.MarkAllAsReadAsync(CurrentUserId);
            }

            return Ok();
        }
    }
}