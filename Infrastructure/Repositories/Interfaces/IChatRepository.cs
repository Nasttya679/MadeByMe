using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<Chat> GetOrCreateChatAsync(string user1Id, string user2Id);

        Task<List<Chat>> GetUserChatsAsync(string userId);

        Task<List<ChatMessage>> GetChatMessagesAsync(int chatId, string userId);

        Task SaveMessageAsync(ChatMessage message);

        Task<ChatMessage?> GetMessageByIdAsync(int messageId);

        Task UpdateMessageAsync(ChatMessage message);

        Task<Chat?> GetChatByIdAsync(int chatId);

        Task UpdateChatAsync(Chat chat);
    }
}