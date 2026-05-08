using System.Collections.Concurrent;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Infrastructure.Data; // Потрібно для доступу до бази
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MadeByMe.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> ConnectedUsers = new();

        private readonly IChatService _chatService;
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatHub(IChatService chatService, IServiceScopeFactory scopeFactory)
        {
            _chatService = chatService;
            _scopeFactory = scopeFactory;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                var userConnections = ConnectedUsers.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
                userConnections.TryAdd(Context.ConnectionId, 0);

                await Clients.All.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                if (ConnectedUsers.TryGetValue(userId, out var connections))
                {
                    connections.TryRemove(Context.ConnectionId, out _);
                    if (connections.IsEmpty)
                    {
                        ConnectedUsers.TryRemove(userId, out _);
                        await Clients.All.SendAsync("UserOffline", userId);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public bool CheckUserOnline(string userId)
        {
            return ConnectedUsers.ContainsKey(userId) && !ConnectedUsers[userId].IsEmpty;
        }

        public async Task JoinChat(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task MarkAsRead(int chatId, string interlocutorId)
        {
            var currentUserId = Context.UserIdentifier;
            if (currentUserId == null)
            {
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var unreadMessages = await dbContext.ChatMessages
                    .Where(m => m.ChatId == chatId && m.SenderId == interlocutorId && !m.IsRead)
                    .ToListAsync();

                if (unreadMessages.Any())
                {
                    foreach (var msg in unreadMessages)
                    {
                        msg.IsRead = true;
                    }

                    await dbContext.SaveChangesAsync();

                    await Clients.User(interlocutorId).SendAsync("MessagesWereRead", chatId);
                }
            }
        }
    }
}