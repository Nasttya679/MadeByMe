using System.Collections.Generic;
using System.Threading.Tasks;
using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class PostServiceTests
    {
        private readonly Mock<IPostRepository> _postRepoMock;
        private readonly Mock<IOptions<ProjectSettings>> _optionsMock;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            Log.Logger = Serilog.Core.Logger.None;
            _postRepoMock = new Mock<IPostRepository>();
            _optionsMock = new Mock<IOptions<ProjectSettings>>();

            var settings = new ProjectSettings();
            settings.Pagination.DefaultPageSize = 10;
            settings.Pagination.MinSearchLength = 3;
            _optionsMock.Setup(o => o.Value).Returns(settings);

            _postService = new PostService(_postRepoMock.Object, _optionsMock.Object);
        }

        [Fact]
        public async Task GetAllPostsAsync_WhenPostsExist_ShouldReturnOnlyActivePosts()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, IsDeleted = false },
                new Post { Id = 2, IsDeleted = true },
                new Post { Id = 3, IsDeleted = false },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetAllPostsAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task UpdatePostAsync_Existing_ShouldUpdateFields()
        {
            var existing = new Post { Id = 1, Title = "Old", IsDeleted = false };
            _postRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);

            _postRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var result = await _postService.UpdatePostAsync(1, new UpdatePostDto { Title = "New" });

            Assert.True(result.IsSuccess);
            Assert.Equal("New", existing.Title);
        }

        [Fact]
        public async Task UpdatePostAsync_DeletedPost_ShouldReturnFailure()
        {
            var deletedPost = new Post { Id = 1, Title = "Old", IsDeleted = true };
            _postRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(deletedPost);

            var result = await _postService.UpdatePostAsync(1, new UpdatePostDto { Title = "New" });

            Assert.True(result.IsFailure);
            _postRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Post>()), Times.Never);
        }

        [Fact]
        public async Task GetAllPostsAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Post>());
            var result = await _postService.GetAllPostsAsync();
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetPostByIdAsync_Existing_ShouldReturnPost()
        {
            int id = 1;
            _postRepoMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(new Post { Id = id, Title = "Handmade Vase" });

            var result = await _postService.GetPostByIdAsync(id);

            Assert.True(result.IsSuccess);
            Assert.Equal("Handmade Vase", result.Value.Title);
        }

        [Fact]
        public async Task GetPostByIdAsync_NotExisting_ShouldReturnFailure()
        {
            _postRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post)null!);

            var result = await _postService.GetPostByIdAsync(999);

            Assert.True(result.IsFailure);
            Assert.Contains("не знайдено", result.ErrorMessage);
        }

        [Fact]
        public async Task CreatePostAsync_ValidData_ShouldReturnSuccess()
        {
            var dto = new CreatePostDto { Title = "Wood Table", Price = 500 };

            _postRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var result = await _postService.CreatePostAsync(dto, "seller-1");

            Assert.True(result.IsSuccess);
            Assert.Equal("Wood Table", result.Value.Title);
        }

        [Fact]
        public async Task CreatePostAsync_ShouldCallRepositoryAddOnce()
        {
            var dto = new CreatePostDto { Title = "Test Item" };
            _postRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            await _postService.CreatePostAsync(dto, "seller-1");

            _postRepoMock.Verify(repo => repo.AddAsync(It.Is<Post>(p => p.Title == "Test Item" && p.SellerId == "seller-1")), Times.Once);
        }

        [Fact]
        public async Task UpdatePostAsync_NotExisting_ShouldReturnFailure()
        {
            _postRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post)null!);

            var result = await _postService.UpdatePostAsync(99, new UpdatePostDto());

            Assert.True(result.IsFailure);
            _postRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Post>()), Times.Never);
        }

        [Fact]
        public async Task SearchPostsAsync_WhenTermMatchesTitle_ShouldReturnResults()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, Title = "Apple Vase", Description = "Red fruit", Price = 100, CategoryId = 1, IsDeleted = false },
                new Post { Id = 2, Title = "Banana Cup", Description = "Yellow", Price = 50, CategoryId = 1, IsDeleted = false },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.SearchPostsAsync("Apple");

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal("Apple Vase", result.Value[0].Title);
        }

        [Fact]
        public async Task GetFilteredPostsAsync_WhenTermMatchesTitle_ShouldReturnResults()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, Title = "Apple Vase", Description = "Decor", Price = 100, CategoryId = 1, IsDeleted = false },
                new Post { Id = 2, Title = "Banana Cup", Description = "Kitchen", Price = 50, CategoryId = 1, IsDeleted = false },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetFilteredPostsAsync("Apple", null, null);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal("Apple Vase", result.Value[0].Title);
        }

        [Fact]
        public async Task DeletePostAsync_Existing_ShouldPerformSoftDelete()
        {
            var post = new Post { Id = 1, IsDeleted = false };
            _postRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(post);
            _postRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var result = await _postService.DeletePostAsync(1, "test_user_id");

            Assert.True(result.IsSuccess);
            Assert.True(post.IsDeleted);
            Assert.Equal("test_user_id", post.DeletedByUserId);

            _postRepoMock.Verify(repo => repo.UpdateAsync(post), Times.Once);
            _postRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<Post>()), Times.Never);
        }

        [Fact]
        public async Task DeletePostAsync_NotExisting_ShouldReturnFailure()
        {
            _postRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post)null!);

            var result = await _postService.DeletePostAsync(99, "test_user_id");

            Assert.True(result.IsFailure);
            _postRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Post>()), Times.Never);
        }

        [Fact]
        public async Task GetDeletedPostsAsync_WithoutUserId_ShouldReturnAllDeletedPosts()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, IsDeleted = true, DeletedByUserId = "user1" },
                new Post { Id = 2, IsDeleted = false },
                new Post { Id = 3, IsDeleted = true, DeletedByUserId = "user2" },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetDeletedPostsAsync(null);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task GetDeletedPostsAsync_WithUserId_ShouldReturnOnlyUserDeletedPosts()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, IsDeleted = true, DeletedByUserId = "user_target" },
                new Post { Id = 2, IsDeleted = true, DeletedByUserId = "user_other" },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetDeletedPostsAsync("user_target");

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal(1, result.Value[0].Id);
        }

        [Fact]
        public async Task RestorePostAsync_ExistingDeletedPost_ShouldRestoreProperly()
        {
            var post = new Post { Id = 1, IsDeleted = true, DeletedByUserId = "some_user" };
            _postRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(post);
            _postRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var result = await _postService.RestorePostAsync(1);

            Assert.True(result.IsSuccess);
            Assert.False(post.IsDeleted);
            Assert.Null(post.DeletedByUserId);
            _postRepoMock.Verify(repo => repo.UpdateAsync(post), Times.Once);
        }

        [Fact]
        public async Task RestorePostAsync_NotExisting_ShouldReturnFailure()
        {
            _postRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post)null!);

            var result = await _postService.RestorePostAsync(99);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task HardDeletePostAsync_Existing_ShouldCallDeleteAsync()
        {
            var post = new Post { Id = 1, IsDeleted = true };
            _postRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(post);
            _postRepoMock.Setup(repo => repo.DeleteAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var result = await _postService.HardDeletePostAsync(1);

            Assert.True(result.IsSuccess);
            _postRepoMock.Verify(repo => repo.DeleteAsync(post), Times.Once);
        }

        [Fact]
        public async Task HardDeletePostAsync_NotExisting_ShouldReturnFailure()
        {
            _postRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post)null!);

            var result = await _postService.HardDeletePostAsync(99);

            Assert.True(result.IsFailure);
            _postRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<Post>()), Times.Never);
        }

        [Fact]
        public async Task GetTopRatedPostsAsync_ShouldReturnHighestRatedActivePosts()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, Title = "Середній", Rating = 4.0m, IsDeleted = false },
                new Post { Id = 2, Title = "Найкращий", Rating = 5.0m, IsDeleted = false },
                new Post { Id = 3, Title = "Дуже крутий, АЛЕ ВИДАЛЕНИЙ", Rating = 5.0m, IsDeleted = true },
                new Post { Id = 4, Title = "Так собі", Rating = 3.0m, IsDeleted = false },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetTopRatedPostsAsync(2);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal(2, result.Value[0].Id);
            Assert.Equal(1, result.Value[1].Id);
            Assert.DoesNotContain(result.Value, p => p.Id == 3);
        }

        [Fact]
        public async Task GetTopRatedPostsAsync_WhenCountIsGreaterThanAvailable_ShouldReturnAllActiveSorted()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, Rating = 4.0m, IsDeleted = false },
                new Post { Id = 2, Rating = 5.0m, IsDeleted = true },
                new Post { Id = 3, Rating = 4.8m, IsDeleted = false },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetTopRatedPostsAsync(5);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal(3, result.Value[0].Id);
            Assert.Equal(1, result.Value[1].Id);
        }

        [Fact]
        public async Task GetTopRatedPostsAsync_WhenNoActivePosts_ShouldReturnEmptyList()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, Rating = 5.0m, IsDeleted = true },
                new Post { Id = 2, Rating = 4.0m, IsDeleted = true },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetTopRatedPostsAsync(4);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }
    }
}