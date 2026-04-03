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
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock;
        private readonly Mock<IPostRepository> _postRepoMock;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            Log.Logger = Serilog.Core.Logger.None;
            _commentRepoMock = new Mock<ICommentRepository>();
            _postRepoMock = new Mock<IPostRepository>();

            _commentService = new CommentService(_commentRepoMock.Object, _postRepoMock.Object);
        }

        [Fact]
        public async Task GetCommentsForPostAsync_WhenCommentsExist_ShouldReturnList()
        {
            int postId = 1;
            var comments = new List<Comment> { new Comment { CommentId = 1, PostId = postId }, new Comment { CommentId = 2, PostId = postId } };
            _commentRepoMock.Setup(repo => repo.GetByPostIdAsync(postId)).ReturnsAsync(comments);

            var result = await _commentService.GetCommentsForPostAsync(postId);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task GetCommentsForPostAsync_WhenNoComments_ShouldReturnEmptyList()
        {
            _commentRepoMock.Setup(repo => repo.GetByPostIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Comment>());

            var result = await _commentService.GetCommentsForPostAsync(99);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetCommentsForPostAsync_ShouldCallRepositoryWithCorrectPostId()
        {
            int postId = 10;
            _commentRepoMock.Setup(repo => repo.GetByPostIdAsync(postId)).ReturnsAsync(new List<Comment>());

            await _commentService.GetCommentsForPostAsync(postId);
            _commentRepoMock.Verify(repo => repo.GetByPostIdAsync(postId), Times.Once);
        }

        [Fact]
        public async Task GetCommentByIdAsync_WhenExists_ShouldReturnComment()
        {
            int commentId = 1;
            _commentRepoMock.Setup(repo => repo.GetByIdAsync(commentId)).ReturnsAsync(new Comment { CommentId = commentId, Content = "Nice!" });

            var result = await _commentService.GetCommentByIdAsync(commentId);

            Assert.True(result.IsSuccess);
            Assert.Equal("Nice!", result.Value.Content);
        }

        [Fact]
        public async Task GetCommentByIdAsync_WhenNotExists_ShouldReturnFailure()
        {
            _commentRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Comment)null!);

            var result = await _commentService.GetCommentByIdAsync(404);

            Assert.True(result.IsFailure);
            Assert.Contains("не знайдено", result.ErrorMessage);
        }

        [Fact]
        public async Task GetCommentByIdAsync_ShouldCallGetByIdOnce()
        {
            _commentRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(new Comment());

            await _commentService.GetCommentByIdAsync(1);
            _commentRepoMock.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_WithValidData_ShouldReturnSuccess()
        {
            var dto = new CreateCommentDto { PostId = 1, Content = "Great post!" };
            string userId = "user-123";

            _commentRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

            var result = await _commentService.AddCommentAsync(dto, userId);

            Assert.True(result.IsSuccess);
            Assert.Equal("Great post!", result.Value.Content);
            Assert.Equal(userId, result.Value.UserId);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldCallAddInRepository()
        {
            var dto = new CreateCommentDto { PostId = 5, Content = "Hello" };
            _commentRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

            await _commentService.AddCommentAsync(dto, "u1");

            _commentRepoMock.Verify(repo => repo.AddAsync(It.Is<Comment>(c => c.Content == "Hello" && c.PostId == 5)), Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldReturnEntityWithCorrectPostId()
        {
            var dto = new CreateCommentDto { PostId = 100 };
            _commentRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

            var result = await _commentService.AddCommentAsync(dto, "user1");

            Assert.Equal(100, result.Value.PostId);
        }

        [Fact]
        public async Task DeleteCommentAsync_Existing_ShouldReturnSuccess()
        {
            int commentId = 1;
            var comment = new Comment { CommentId = commentId };
            _commentRepoMock.Setup(repo => repo.GetByIdAsync(commentId)).ReturnsAsync(comment);
            _commentRepoMock.Setup(repo => repo.DeleteAsync(comment)).Returns(Task.CompletedTask);

            var result = await _commentService.DeleteCommentAsync(commentId);

            Assert.True(result.IsSuccess);
            _commentRepoMock.Verify(repo => repo.DeleteAsync(comment), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_NonExisting_ShouldReturnFailure()
        {
            _commentRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Comment)null!);

            var result = await _commentService.DeleteCommentAsync(99);

            Assert.True(result.IsFailure);
            _commentRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldVerifyExistenceBeforeDelete()
        {
            int id = 10;
            _commentRepoMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(new Comment { CommentId = id });
            _commentRepoMock.Setup(repo => repo.DeleteAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

            await _commentService.DeleteCommentAsync(id);
            _commentRepoMock.Verify(repo => repo.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldUpdatePostRatingAutomatically()
        {
            var post = new Post { Id = 1, Rating = 0 };
            var dto = new CreateCommentDto { PostId = 1, Stars = 4, Content = "Good" };

            var commentsAfterAdding = new List<Comment>
            {
                new Comment { Stars = 5 },
                new Comment { Stars = 4 },
            };

            _postRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(post);
            _commentRepoMock.Setup(repo => repo.GetByPostIdAsync(1)).ReturnsAsync(commentsAfterAdding);
            _commentRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
            _postRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);

            await _commentService.AddCommentAsync(dto, "user-1");

            Assert.Equal(4.5m, post.Rating);
            _postRepoMock.Verify(repo => repo.UpdateAsync(post), Times.Once);
        }
    }
}