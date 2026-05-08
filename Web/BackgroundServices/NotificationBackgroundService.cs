using MadeByMe.Application.DTOs;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MadeByMe.Web.BackgroundServices
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(
            IServiceScopeFactory scopeFactory,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                        var unsent = await repo.GetUnsentNotificationsAsync();

                        if (unsent.Any())
                        {
                            foreach (var n in unsent)
                            {
                                var dto = new NotificationDto
                                {
                                    Id = n.Id,
                                    Message = n.Message,
                                    ActionUrl = n.ActionUrl,
                                    CreatedAt = n.CreatedAt,
                                    SenderAvatar = n.SenderAvatar,
                                };

                                await _hubContext.Clients.Group(n.UserId).SendAsync("ReceiveNotification", dto, stoppingToken);
                            }

                            await repo.MarkAsSentAsync(unsent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка під час відправки фонових сповіщень");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}