using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;

namespace MadeByMe.Tests.Services
{
    public class PostServiceTests
    {
        private readonly Mock<IPostRepository> _postRepoMock;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            _postRepoMock = new Mock<IPostRepository>();
            _postService = new PostService(_postRepoMock.Object);
        }

        [Fact]
        public void GetAllPosts_WhenPostsExist_ShouldReturnSuccess()
        {
            var posts = new List<Post> { new Post { Id = 1 }, new Post { Id = 2 } };
            _postRepoMock.Setup(repo => repo.GetAll()).Returns(posts);

            var result = _postService.GetAllPosts();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public void GetAllPosts_WhenEmpty_ShouldReturnEmptyList()
        {
            _postRepoMock.Setup(repo => repo.GetAll()).Returns(new List<Post>());
            var result = _postService.GetAllPosts();
            Assert.Empty(result.Value);
        }

        [Fact]
        public void GetAllPosts_ShouldCallRepoOnce()
        {
            _postService.GetAllPosts();
            _postRepoMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public void GetPostById_Existing_ShouldReturnPost()
        {
            int id = 1;
            _postRepoMock.Setup(repo => repo.GetById(id)).Returns(new Post { Id = id, Title = "Handmade Vase" });

            var result = _postService.GetPostById(id);

            Assert.True(result.IsSuccess);
            Assert.Equal("Handmade Vase", result.Value.Title);
        }

        [Fact]
        public void GetPostById_NonExisting_ShouldReturnFailure()
        {
            _postRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Post)null!);
            var result = _postService.GetPostById(999);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void GetPostById_ShouldCallGetByIdWithCorrectId()
        {
            _postService.GetPostById(5);
            _postRepoMock.Verify(repo => repo.GetById(5), Times.Once);
        }

        [Fact]
        public void CreatePost_ValidData_ShouldReturnSuccess()
        {
            var dto = new CreatePostDto { Title = "Wood Table", Price = 500 };
            var result = _postService.CreatePost(dto, "seller-1");

            Assert.True(result.IsSuccess);
            Assert.Equal("Wood Table", result.Value.Title);
            Assert.Equal("seller-1", result.Value.SellerId);
        }

        [Fact]
        public void CreatePost_ShouldCallAddMethod()
        {
            var dto = new CreatePostDto { Title = "Chair" };
            _postService.CreatePost(dto, "s1");
            _postRepoMock.Verify(repo => repo.Add(It.IsAny<Post>()), Times.Once);
        }

        [Fact]
        public void CreatePost_ShouldMapAllFieldsCorrectly()
        {
            var dto = new CreatePostDto { Title = "T", Description = "D", Price = 10, CategoryId = 1 };
            var result = _postService.CreatePost(dto, "s1");

            Assert.Equal(10, result.Value.Price);
            Assert.Equal(1, result.Value.CategoryId);
        }

        [Fact]
        public void UpdatePost_Existing_ShouldUpdateFields()
        {
            var existing = new Post { Id = 1, Title = "Old" };
            _postRepoMock.Setup(repo => repo.GetById(1)).Returns(existing);

            var result = _postService.UpdatePost(1, new UpdatePostDto { Title = "New" });

            Assert.True(result.IsSuccess);
            Assert.Equal("New", existing.Title);
        }

        [Fact]
        public void UpdatePost_NonExisting_ShouldReturnFailure()
        {
            _postRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Post)null!);
            var result = _postService.UpdatePost(1, new UpdatePostDto());
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void UpdatePost_WhenFieldsAreNull_ShouldKeepOriginalValues()
        {
            var existing = new Post { Id = 1, Title = "Keep Me", Price = 100 };
            _postRepoMock.Setup(repo => repo.GetById(1)).Returns(existing);

            _postService.UpdatePost(1, new UpdatePostDto { Title = null, Price = null });

            Assert.Equal("Keep Me", existing.Title);
            Assert.Equal(100, existing.Price);
        }

        [Fact]
        public void DeletePost_Existing_ShouldReturnSuccess()
        {
            var post = new Post { Id = 1 };
            _postRepoMock.Setup(repo => repo.GetById(1)).Returns(post);

            var result = _postService.DeletePost(1);

            Assert.True(result.IsSuccess);
            _postRepoMock.Verify(repo => repo.Delete(post), Times.Once);
        }

        [Fact]
        public void DeletePost_NonExisting_ShouldReturnFailure()
        {
            _postRepoMock.Setup(repo => repo.GetById(1)).Returns((Post)null!);
            var result = _postService.DeletePost(1);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void DeletePost_ShouldCheckExistenceFirst()
        {
            _postService.DeletePost(10);
            _postRepoMock.Verify(repo => repo.GetById(10), Times.Once);
        }

        [Fact]
        public void SearchPosts_WhenTermMatchesTitle_ShouldReturnResults()
        {
            var posts = new List<Post>
            {
                new Post { Title = "Apple", Description = "Fresh fruit" },
                new Post { Title = "Banana", Description = "Yellow fruit" },
            };
            _postRepoMock.Setup(repo => repo.GetAll()).Returns(posts);

            var result = _postService.SearchPosts("Apple");

            Assert.Single(result.Value);
            Assert.Equal("Apple", result.Value[0].Title);
        }

        [Fact]
        public void SearchPosts_WhenEmptyTerm_ShouldReturnAllPosts()
        {
            var posts = new List<Post> { new Post { Title = "A" }, new Post { Title = "B" } };
            _postRepoMock.Setup(repo => repo.GetAll()).Returns(posts);

            var result = _postService.SearchPosts(string.Empty);

            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public void SearchPosts_WhenNoMatches_ShouldReturnEmptyList()
        {
            var posts = new List<Post> { new Post { Title = "A", Description = "Desc" } };
            _postRepoMock.Setup(repo => repo.GetAll()).Returns(posts);

            var result = _postService.SearchPosts("NonExistent");

            Assert.Empty(result.Value);
        }
    }
}