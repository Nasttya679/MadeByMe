using MadeByMe.Application.DTOs.Chat;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatRepository> _chatRepoMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _chatRepoMock = new Mock<IChatRepository>();
            _envMock = new Mock<IWebHostEnvironment>();

            _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

            _chatService = new ChatService(_chatRepoMock.Object, _envMock.Object);
        }

        [Fact]
        public async Task GetOrCreateChatAsync_ShouldReturnChat_WhenCalled()
        {
            var expectedChat = new Chat { Id = 1, BuyerId = "user1", SellerId = "user2" };
            _chatRepoMock.Setup(r => r.GetOrCreateChatAsync("user1", "user2"))
                         .ReturnsAsync(expectedChat);

            var result = await _chatService.GetOrCreateChatAsync("user1", "user2");

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedChat.Id, result.Value.Id);
            _chatRepoMock.Verify(r => r.GetOrCreateChatAsync("user1", "user2"), Times.Once);
        }

        [Fact]
        public async Task GetUserChatsAsync_ShouldMapInterlocutorCorrectly_ForBuyer()
        {
            var userId = "buyerId";
            var chats = new List<Chat>
            {
                new Chat
                {
                    Id = 1, BuyerId = userId, SellerId = "sellerId",
                    Seller = new ApplicationUser { UserName = "SellerName" },
                    Messages = new List<ChatMessage>(),
                },
            };
            _chatRepoMock.Setup(r => r.GetUserChatsAsync(userId)).ReturnsAsync(chats);

            var result = await _chatService.GetUserChatsAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal("SellerName", result.Value[0].InterlocutorName);
            Assert.Equal("Чат порожній", result.Value[0].LastMessage);
        }

        [Fact]
        public async Task GetUserChatsAsync_ShouldReturnFile_WhenLastMessageIsAttachmentOnly()
        {
            var userId = "sellerId";
            var chats = new List<Chat>
            {
                new Chat
                {
                    Id = 1, BuyerId = "buyerId", SellerId = userId,
                    Messages = new List<ChatMessage>
                    {
                        new ChatMessage { Content = string.Empty, FilePath = "/path.jpg", CreatedAt = DateTime.UtcNow, },
                    },
                },
            };
            _chatRepoMock.Setup(r => r.GetUserChatsAsync(userId)).ReturnsAsync(chats);

            var result = await _chatService.GetUserChatsAsync(userId);

            Assert.Equal("Файл", result.Value[0].LastMessage);
        }

        [Fact]
        public async Task GetChatMessagesAsync_ShouldReturnMessages()
        {
            var messages = new List<ChatMessage> { new ChatMessage { Id = 1, Content = "Test" } };
            _chatRepoMock.Setup(r => r.GetChatMessagesAsync(1, "user1")).ReturnsAsync(messages);

            var result = await _chatService.GetChatMessagesAsync(1, "user1");

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal("Test", result.Value[0].Content);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldSaveTextMessage_WhenNoAttachment()
        {
            var dto = new ChatMessageDto { ChatId = 1, Content = "Hello" };

            var result = await _chatService.SendMessageAsync("sender1", dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Hello", result.Value.Content);
            Assert.Null(result.Value.FilePath);
            _chatRepoMock.Verify(r => r.SaveMessageAsync(It.IsAny<ChatMessage>()), Times.Once);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldProcessAttachment_WhenFileIsProvided()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var dto = new ChatMessageDto { ChatId = 1, Content = "Look!", Attachment = fileMock.Object };

            var result = await _chatService.SendMessageAsync("sender1", dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Image", result.Value.FileType);
            Assert.Contains("test.jpg", result.Value.FilePath);
            _chatRepoMock.Verify(r => r.SaveMessageAsync(It.IsAny<ChatMessage>()), Times.Once);
        }

        [Fact]
        public async Task TogglePinChatAsync_ShouldReturnFailure_WhenChatNotFound()
        {
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(99)).ReturnsAsync((Chat)null!);

            var result = await _chatService.TogglePinChatAsync(99, "user1");

            Assert.True(result.IsFailure);
            Assert.Equal("Чат не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public async Task TogglePinChatAsync_ShouldTogglePinForBuyer()
        {
            var chat = new Chat { Id = 1, BuyerId = "buyer1", IsPinnedByBuyer = false };
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(chat);

            var result = await _chatService.TogglePinChatAsync(1, "buyer1");

            Assert.True(result.IsSuccess);
            Assert.True(chat.IsPinnedByBuyer);
            _chatRepoMock.Verify(r => r.UpdateChatAsync(chat), Times.Once);
        }

        [Fact]
        public async Task DeleteChatAsync_ShouldDeleteForEveryone_WhenFlagIsTrue()
        {
            var chat = new Chat
            {
                Id = 1,
                Messages = new List<ChatMessage> { new ChatMessage { Id = 1 }, },
            };
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(chat);

            var result = await _chatService.DeleteChatAsync(1, "user1", forEveryone: true);

            Assert.True(result.IsSuccess);
            Assert.True(chat.IsDeletedForEveryone);
            Assert.True(chat.Messages.First().IsDeletedBySender);
            Assert.True(chat.Messages.First().IsDeletedByRecipient);
            _chatRepoMock.Verify(r => r.UpdateChatAsync(chat), Times.Once);
        }

        [Fact]
        public async Task DeleteChatAsync_ShouldDeleteOnlyForSeller_WhenFlagIsFalse()
        {
            var chat = new Chat
            {
                Id = 1,
                BuyerId = "buyer",
                SellerId = "seller",
                Messages = new List<ChatMessage> { new ChatMessage { SenderId = "buyer" }, },
            };
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(chat);

            var result = await _chatService.DeleteChatAsync(1, "seller", forEveryone: false);

            Assert.True(result.IsSuccess);
            Assert.True(chat.IsDeletedBySeller);
            Assert.False(chat.IsDeletedByBuyer);
            Assert.True(chat.Messages.First().IsDeletedByRecipient); // Seller is recipient here
        }

        [Fact]
        public async Task DeleteChatAsync_ShouldReturnFailure_WhenChatNotFound()
        {
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync((Chat)null!);

            var result = await _chatService.DeleteChatAsync(1, "user1", false);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldUpdateContentAndSetFlag_WhenForEveryone()
        {
            var message = new ChatMessage { Id = 1, Content = "Secret", FilePath = "/img.jpg" };
            _chatRepoMock.Setup(r => r.GetMessageByIdAsync(1)).ReturnsAsync(message);

            var result = await _chatService.DeleteMessageAsync(1, "user1", forEveryone: true);

            Assert.True(result.IsSuccess);
            Assert.True(message.IsDeletedForEveryone);
            Assert.Equal("Повідомлення видалено", message.Content);
            Assert.Null(message.FilePath);
            _chatRepoMock.Verify(r => r.UpdateMessageAsync(message), Times.Once);
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldSetSenderFlag_WhenNotForEveryone()
        {
            var message = new ChatMessage { Id = 1, SenderId = "senderId" };
            _chatRepoMock.Setup(r => r.GetMessageByIdAsync(1)).ReturnsAsync(message);

            var result = await _chatService.DeleteMessageAsync(1, "senderId", forEveryone: false);

            Assert.True(result.IsSuccess);
            Assert.True(message.IsDeletedBySender);
            Assert.False(message.IsDeletedByRecipient);
            _chatRepoMock.Verify(r => r.UpdateMessageAsync(message), Times.Once);
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnFailure_WhenMessageNotFound()
        {
            _chatRepoMock.Setup(r => r.GetMessageByIdAsync(99)).ReturnsAsync((ChatMessage)null!);

            var result = await _chatService.DeleteMessageAsync(99, "user1", true);

            Assert.True(result.IsFailure);
            Assert.Equal("Повідомлення не знайдено.", result.ErrorMessage);
        }
    }
}