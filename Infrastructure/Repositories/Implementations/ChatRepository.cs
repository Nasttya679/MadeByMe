using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Chat> GetOrCreateChatAsync(string user1Id, string user2Id)
        {
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c =>
                    (c.BuyerId == user1Id && c.SellerId == user2Id) ||
                    (c.BuyerId == user2Id && c.SellerId == user1Id));

            if (chat == null)
            {
                chat = new Chat
                {
                    BuyerId = user1Id,
                    SellerId = user2Id,
                    LastMessageAt = DateTime.UtcNow
                };
                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (chat.IsDeletedForEveryone)
                {
                    chat.IsPinnedByBuyer = false;
                    chat.IsPinnedBySeller = false;
                }

                if (chat.IsDeletedByBuyer)
                {
                    chat.IsPinnedByBuyer = false;
                }

                if (chat.IsDeletedBySeller)
                {
                    chat.IsPinnedBySeller = false;
                }

                chat.IsDeletedByBuyer = false;
                chat.IsDeletedBySeller = false;
                chat.IsDeletedForEveryone = false;

                await _context.SaveChangesAsync();
            }

            return chat;
        }

        public async Task<List<Chat>> GetUserChatsAsync(string userId)
        {
            return await _context.Chats
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages
                    .Where(m => !m.IsDeletedForEveryone &&
                                ((m.SenderId == userId && !m.IsDeletedBySender) ||
                                 (m.SenderId != userId && !m.IsDeletedByRecipient)))
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(1))
                .Where(c => !c.IsDeletedForEveryone &&
                            ((c.BuyerId == userId && !c.IsDeletedByBuyer) ||
                             (c.SellerId == userId && !c.IsDeletedBySeller)))
                .OrderByDescending(c => c.BuyerId == userId ? c.IsPinnedByBuyer : c.IsPinnedBySeller)
                .ThenByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync(int chatId, string userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatId == chatId)
                .Where(m => (m.SenderId == userId && !m.IsDeletedBySender) ||
                            (m.SenderId != userId && !m.IsDeletedByRecipient))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);

            var chat = await _context.Chats.FindAsync(message.ChatId);
            if (chat != null)
            {
                chat.LastMessageAt = DateTime.UtcNow;
                chat.IsDeletedByBuyer = false;
                chat.IsDeletedBySeller = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(int messageId)
        {
            return await _context.ChatMessages.FindAsync(messageId);
        }

        public async Task UpdateMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task<Chat?> GetChatByIdAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task UpdateChatAsync(Chat chat)
        {
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();
        }
    }
}