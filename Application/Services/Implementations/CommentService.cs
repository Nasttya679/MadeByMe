using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public Result<List<Comment>> GetCommentsForPost(int postId)
        {
            var comments = _commentRepository.GetByPostId(postId);
            Log.Information("Отримано список коментарів для поста {PostId}. Кількість коментарів: {Count}", postId, comments.Count);
            return Result<List<Comment>>.Success(comments);
        }

        public Result<Comment> GetCommentById(int id)
        {
            var comment = _commentRepository.GetById(id);

            if (comment == null)
            {
                Log.Warning("Коментар з ID {CommentId} не знайдено", id);
                return Result<Comment>.Failure($"Коментар з ID {id} не знайдено.");
            }

            return Result<Comment>.Success(comment);
        }

        public Result<Comment> AddComment(CreateCommentDto dto, string userId)
        {
            Log.Information("Початок додавання коментаря до поста {PostId} користувачем {UserId}", dto.PostId, userId);

            var comment = new Comment
            {
                UserId = userId,
                PostId = dto.PostId,
                Content = dto.Content,
            };

            _commentRepository.Add(comment);

            Log.Information("Коментар успішно додано до поста {PostId}. ID коментаря: {CommentId}", dto.PostId, comment.CommentId);
            return Result<Comment>.Success(comment);
        }

        public Result DeleteComment(int id)
        {
            Log.Information("Видалення коментаря з ID {CommentId}", id);

            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                Log.Warning("Невдала спроба видалення: коментар з ID {CommentId} не знайдено", id);
                return Result.Failure("Коментар для видалення не знайдено.");
            }

            _commentRepository.Delete(comment);

            Log.Information("Коментар {CommentId} успішно видалено", id);
            return Result.Success();
        }
    }
}