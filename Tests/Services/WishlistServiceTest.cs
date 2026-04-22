using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class WishlistServiceTest
    {
        private readonly Mock<IWishlistRepository> _mockRepo;
        private readonly Mock<ILogger<WishlistService>> _mockLogger;
        private readonly Mock<IOptions<ProjectSettings>> _mockOptions;
        private readonly WishlistService _wishlistService;

        public WishlistServiceTest()
        {
            _mockRepo = new Mock<IWishlistRepository>();
            _mockLogger = new Mock<ILogger<WishlistService>>();
            _mockOptions = new Mock<IOptions<ProjectSettings>>();

            var settings = new ProjectSettings
            {
                FileStorage = new FileStorageSettings { DefaultImagePath = "/images/default-test.jpg", },
            };
            _mockOptions.Setup(o => o.Value).Returns(settings);

            _wishlistService = new WishlistService(_mockRepo.Object, _mockLogger.Object, _mockOptions.Object);
        }

        [Fact]
        public async Task ToggleFavoriteAsync_WhenItemDoesNotExist_ShouldAddAndReturnTrue()
        {
            string userId = "user123";
            int postId = 1;

            _mockRepo.Setup(r => r.GetWishlistItemAsync(userId, postId))
                     .ReturnsAsync((Wishlist?)null);

            _mockRepo.Setup(r => r.GetCountAsync(userId))
                     .ReturnsAsync(1);

            var result = await _wishlistService.ToggleFavoriteAsync(userId, postId);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.IsAdded);
            Assert.Equal(1, result.Value.TotalCount);

            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Wishlist>()), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockRepo.Verify(r => r.Remove(It.IsAny<Wishlist>()), Times.Never);
        }

        [Fact]
        public async Task ToggleFavoriteAsync_WhenItemExists_ShouldRemoveAndReturnFalse()
        {
            string userId = "user123";
            int postId = 1;
            var existingItem = new Wishlist { UserId = userId, PostId = postId };

            _mockRepo.Setup(r => r.GetWishlistItemAsync(userId, postId))
                     .ReturnsAsync(existingItem);

            _mockRepo.Setup(r => r.GetCountAsync(userId))
                     .ReturnsAsync(0);

            var result = await _wishlistService.ToggleFavoriteAsync(userId, postId);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.IsAdded);
            Assert.Equal(0, result.Value.TotalCount);

            _mockRepo.Verify(r => r.Remove(existingItem), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Wishlist>()), Times.Never);
        }

        [Fact]
        public async Task GetUserWishlistAsync_ShouldReturnMappedDtos()
        {
            string userId = "user123";
            var mockDbList = new List<Wishlist>
            {
                new Wishlist
                {
                    UserId = userId,
                    PostId = 10,
                    AddedAt = DateTime.UtcNow,
                    Post = new Post
                    {
                        Id = 10,
                        Title = "Тестова Шапочка",
                        Price = 500m,
                        Photos = new List<Photo> { new Photo { FilePath = "/images/hat.jpg" }, },
                    },
                },
            };

            _mockRepo.Setup(r => r.GetUserWishlistAsync(userId))
                     .ReturnsAsync(mockDbList);

            var result = await _wishlistService.GetUserWishlistAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);

            var dto = result.Value.First();
            Assert.Equal(10, dto.PostId);
            Assert.Equal("Тестова Шапочка", dto.Title);
            Assert.Equal(500m, dto.Price);
            Assert.Equal("/images/hat.jpg", dto.PhotoUrl);
        }

        [Fact]
        public async Task GetUserWishlistAsync_WhenPostHasNoPhotos_ShouldReturnDefaultImage()
        {
            string userId = "user123";
            var mockDbList = new List<Wishlist>
            {
                new Wishlist
                {
                    UserId = userId,
                    PostId = 99,
                    Post = new Post
                    {
                        Title = "Товар без фото",
                        Price = 100m,
                        Photos = new List<Photo>(),
                    },
                },
            };

            _mockRepo.Setup(r => r.GetUserWishlistAsync(userId))
                     .ReturnsAsync(mockDbList);

            var result = await _wishlistService.GetUserWishlistAsync(userId);

            Assert.True(result.IsSuccess);

            var dto = result.Value.First();
            Assert.Equal("/images/default-test.jpg", dto.PhotoUrl);
        }

        [Fact]
        public async Task GetWishlistCountAsync_ShouldReturnCorrectCount()
        {
            string userId = "user123";
            _mockRepo.Setup(r => r.GetCountAsync(userId)).ReturnsAsync(5);

            var result = await _wishlistService.GetWishlistCountAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.Equal(5, result.Value);
        }
    }
}