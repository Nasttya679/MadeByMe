using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs.Chat;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace MadeByMe.Application.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IWebHostEnvironment _env;

        public ChatService(IChatRepository chatRepo, IWebHostEnvironment env)
        {
            _chatRepo = chatRepo;
            _env = env;
        }

        public async Task<Result<Chat>> GetOrCreateChatAsync(string currentUserId, string targetUserId)
        {
            return await _chatRepo.GetOrCreateChatAsync(currentUserId, targetUserId);
        }

        public async Task<Result<List<ChatSummaryDto>>> GetUserChatsAsync(string userId)
        {
            var chats = await _chatRepo.GetUserChatsAsync(userId);

            var dtos = chats.Select(c =>
            {
                var lastMsg = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
                string lastMessageText = "Чат порожній";

                if (lastMsg != null)
                {
                    lastMessageText = !string.IsNullOrEmpty(lastMsg.Content) ? lastMsg.Content : "Файл";
                }

                return new ChatSummaryDto
                {
                    ChatId = c.Id,
                    InterlocutorId = c.BuyerId == userId ? c.SellerId : c.BuyerId,
                    InterlocutorName = c.BuyerId == userId ? c.Seller?.UserName : c.Buyer?.UserName,
                    InterlocutorPhoto = c.BuyerId == userId ? c.Seller?.ProfilePicture : c.Buyer?.ProfilePicture,
                    LastMessage = lastMessageText,
                    LastMessageTime = c.LastMessageAt,
                    IsPinned = c.BuyerId == userId ? c.IsPinnedByBuyer : c.IsPinnedBySeller,
                };
            }).ToList();

            return dtos;
        }

        public async Task<Result<List<ChatMessage>>> GetChatMessagesAsync(int chatId, string userId)
        {
            return await _chatRepo.GetChatMessagesAsync(chatId, userId);
        }

        public async Task<Result<ChatMessage>> SendMessageAsync(string senderId, ChatMessageDto dto)
        {
            var message = new ChatMessage
            {
                ChatId = dto.ChatId,
                SenderId = senderId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
            };

            if (dto.Attachment != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/chat");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.Attachment.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(fileStream);
                }

                message.FilePath = "/uploads/chat/" + uniqueFileName;
                message.FileName = dto.Attachment.FileName;
                message.FileType = dto.Attachment.ContentType.StartsWith("image") ? "Image" : "Document";
            }

            await _chatRepo.SaveMessageAsync(message);
            return message;
        }

        public async Task<Result> TogglePinChatAsync(int chatId, string userId)
        {
            var chat = await _chatRepo.GetChatByIdAsync(chatId);
            if (chat == null)
            {
                return "Чат не знайдено.";
            }

            if (chat.BuyerId == userId)
            {
                chat.IsPinnedByBuyer = !chat.IsPinnedByBuyer;
            }
            else if (chat.SellerId == userId)
            {
                chat.IsPinnedBySeller = !chat.IsPinnedBySeller;
            }

            await _chatRepo.UpdateChatAsync(chat);
            return Result.Success();
        }

        public async Task<Result> DeleteChatAsync(int chatId, string userId, bool forEveryone)
        {
            var chat = await _chatRepo.GetChatByIdAsync(chatId);
            if (chat == null)
            {
                return "Чат не знайдено.";
            }

            if (forEveryone)
            {
                chat.IsDeletedForEveryone = true;

                foreach (var msg in chat.Messages)
                {
                    msg.IsDeletedBySender = true;
                    msg.IsDeletedByRecipient = true;
                }
            }
            else
            {
                if (chat.BuyerId == userId)
                {
                    chat.IsDeletedByBuyer = true;
                }
                else
                {
                    chat.IsDeletedBySeller = true;
                }

                foreach (var msg in chat.Messages)
                {
                    if (msg.SenderId == userId)
                    {
                        msg.IsDeletedBySender = true;
                    }
                    else
                    {
                        msg.IsDeletedByRecipient = true;
                    }
                }
            }

            await _chatRepo.UpdateChatAsync(chat);
            return Result.Success();
        }

        public async Task<Result> DeleteMessageAsync(int messageId, string userId, bool forEveryone)
        {
            var message = await _chatRepo.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return "Повідомлення не знайдено.";
            }

            if (forEveryone)
            {
                message.IsDeletedForEveryone = true;
                message.Content = "Повідомлення видалено";
                message.FilePath = null;
            }
            else
            {
                if (message.SenderId == userId)
                {
                    message.IsDeletedBySender = true;
                }
                else
                {
                    message.IsDeletedByRecipient = true;
                }
            }

            await _chatRepo.UpdateMessageAsync(message);
            return Result.Success();
        }
    }
}