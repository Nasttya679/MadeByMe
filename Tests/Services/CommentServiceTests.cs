using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;

namespace MadeByMe.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _commentRepoMock = new Mock<ICommentRepository>();
            _commentService = new CommentService(_commentRepoMock.Object);
        }

        [Fact]
        public void GetCommentsForPost_WhenCommentsExist_ShouldReturnList()
        {
            int postId = 1;
            var comments = new List<Comment> { new Comment { CommentId = 1, PostId = postId }, new Comment { CommentId = 2, PostId = postId } };
            _commentRepoMock.Setup(repo => repo.GetByPostId(postId)).Returns(comments);

            var result = _commentService.GetCommentsForPost(postId);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public void GetCommentsForPost_WhenNoComments_ShouldReturnEmptyList()
        {
            _commentRepoMock.Setup(repo => repo.GetByPostId(It.IsAny<int>())).Returns(new List<Comment>());

            var result = _commentService.GetCommentsForPost(99);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public void GetCommentsForPost_ShouldCallRepositoryWithCorrectPostId()
        {
            int postId = 10;
            _commentService.GetCommentsForPost(postId);
            _commentRepoMock.Verify(repo => repo.GetByPostId(postId), Times.Once);
        }

        [Fact]
        public void GetCommentById_WhenExists_ShouldReturnComment()
        {
            int commentId = 1;
            _commentRepoMock.Setup(repo => repo.GetById(commentId)).Returns(new Comment { CommentId = commentId, Content = "Nice!" });

            var result = _commentService.GetCommentById(commentId);

            Assert.True(result.IsSuccess);
            Assert.Equal("Nice!", result.Value.Content);
        }

        [Fact]
        public void GetCommentById_WhenNotExists_ShouldReturnFailure()
        {
            _commentRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Comment)null!);

            var result = _commentService.GetCommentById(404);

            Assert.True(result.IsFailure);
            Assert.Contains("не знайдено", result.ErrorMessage);
        }

        [Fact]
        public void GetCommentById_ShouldCallGetByIdOnce()
        {
            _commentService.GetCommentById(1);
            _commentRepoMock.Verify(repo => repo.GetById(1), Times.Once);
        }

        [Fact]
        public void AddComment_WithValidData_ShouldReturnSuccess()
        {
            var dto = new CreateCommentDto { PostId = 1, Content = "Great post!" };
            string userId = "user-123";

            var result = _commentService.AddComment(dto, userId);

            Assert.True(result.IsSuccess);
            Assert.Equal("Great post!", result.Value.Content);
            Assert.Equal(userId, result.Value.UserId);
        }

        [Fact]
        public void AddComment_ShouldCallAddInRepository()
        {
            var dto = new CreateCommentDto { PostId = 5, Content = "Hello" };

            _commentService.AddComment(dto, "u1");

            _commentRepoMock.Verify(repo => repo.Add(It.Is<Comment>(c => c.Content == "Hello" && c.PostId == 5)), Times.Once);
        }

        [Fact]
        public void AddComment_ShouldReturnEntityWithCorrectPostId()
        {
            var dto = new CreateCommentDto { PostId = 100 };
            var result = _commentService.AddComment(dto, "user1");

            Assert.Equal(100, result.Value.PostId);
        }

        [Fact]
        public void DeleteComment_Existing_ShouldReturnSuccess()
        {
            int commentId = 1;
            var comment = new Comment { CommentId = commentId };
            _commentRepoMock.Setup(repo => repo.GetById(commentId)).Returns(comment);

            var result = _commentService.DeleteComment(commentId);

            Assert.True(result.IsSuccess);
            _commentRepoMock.Verify(repo => repo.Delete(comment), Times.Once);
        }

        [Fact]
        public void DeleteComment_NonExisting_ShouldReturnFailure()
        {
            _commentRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Comment)null!);

            var result = _commentService.DeleteComment(99);

            Assert.True(result.IsFailure);
            _commentRepoMock.Verify(repo => repo.Delete(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public void DeleteComment_ShouldVerifyExistenceBeforeDelete()
        {
            int id = 10;
            _commentService.DeleteComment(id);
            _commentRepoMock.Verify(repo => repo.GetById(id), Times.Once);
        }
    }
}