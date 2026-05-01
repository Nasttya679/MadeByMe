using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs.Chat;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IChatService
    {
        Task<Result<Chat>> GetOrCreateChatAsync(string currentUserId, string targetUserId);

        Task<Result<List<ChatSummaryDto>>> GetUserChatsAsync(string userId);

        Task<Result<List<ChatMessage>>> GetChatMessagesAsync(int chatId, string userId);

        Task<Result<ChatMessage>> SendMessageAsync(string senderId, ChatMessageDto dto);

        Task<Result> TogglePinChatAsync(int chatId, string userId);

        Task<Result> DeleteChatAsync(int chatId, string userId, bool forEveryone);

        Task<Result> DeleteMessageAsync(int messageId, string userId, bool forEveryone);
    }
}