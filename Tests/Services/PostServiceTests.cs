using System.Collections.Generic;
using System.Threading.Tasks;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;
using Serilog;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class PostServiceTests
    {
        private readonly Mock<IPostRepository> _postRepoMock;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            Log.Logger = Serilog.Core.Logger.None;
            _postRepoMock = new Mock<IPostRepository>();
            _postService = new PostService(_postRepoMock.Object);
        }

        [Fact]
        public async Task GetAllPostsAsync_WhenPostsExist_ShouldReturnSuccess()
        {
            var posts = new List<Post> { new Post { Id = 1 }, new Post { Id = 2 } };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetAllPostsAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task GetAllPostsAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Post>());
            var result = await _postService.GetAllPostsAsync();
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetAllPostsAsync_ShouldCallRepoOnce()
        {
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Post>());
            await _postService.GetAllPostsAsync();
            _postRepoMock.Verify(repo => repo.GetAllAsync(), Times.Once);
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
        public async Task CreatePostAsync_ValidData_ShouldReturnSuccess()
        {
            var dto = new CreatePostDto { Title = "Wood Table", Price = 500 };

            _postRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var result = await _postService.CreatePostAsync(dto, "seller-1");

            Assert.True(result.IsSuccess);
            Assert.Equal("Wood Table", result.Value.Title);
        }

        [Fact]
        public async Task UpdatePostAsync_Existing_ShouldUpdateFields()
        {
            var existing = new Post { Id = 1, Title = "Old" };
            _postRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);

            _postRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var result = await _postService.UpdatePostAsync(1, new UpdatePostDto { Title = "New" });

            Assert.True(result.IsSuccess);
            Assert.Equal("New", existing.Title);
        }

        [Fact]
        public async Task DeletePostAsync_Existing_ShouldReturnSuccess()
        {
            var post = new Post { Id = 1 };
            _postRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(post);

            _postRepoMock.Setup(repo => repo.DeleteAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            var result = await _postService.DeletePostAsync(1);

            Assert.True(result.IsSuccess);
            _postRepoMock.Verify(repo => repo.DeleteAsync(post), Times.Once);
        }

        [Fact]
        public async Task SearchPostsAsync_WhenTermMatchesTitle_ShouldReturnResults()
        {
            var posts = new List<Post>
            {
                new Post { Id = 1, Title = "Apple Vase", Description = "Red fruit", Price = 100, CategoryId = 1 },
                new Post { Id = 2, Title = "Banana Cup", Description = "Yellow", Price = 50, CategoryId = 1 },
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
                new Post { Id = 1, Title = "Apple Vase", Description = "Decor", Price = 100, CategoryId = 1 },
                new Post { Id = 2, Title = "Banana Cup", Description = "Kitchen", Price = 50, CategoryId = 1 },
            };
            _postRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(posts);

            var result = await _postService.GetFilteredPostsAsync("Apple", null, null);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal("Apple Vase", result.Value[0].Title);
        }
    }
}