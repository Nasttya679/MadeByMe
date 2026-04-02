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
        private readonly IPostRepository _postRepository;

        public CommentService(ICommentRepository commentRepository, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
        }

        public async Task<Result<List<Comment>>> GetCommentsForPostAsync(int postId)
        {
            var comments = await _commentRepository.GetByPostIdAsync(postId);
            Log.Information("Отримано список коментарів для поста {PostId}. Кількість коментарів: {Count}", postId, comments.Count);
            return comments;
        }

        public async Task<Result<Comment>> GetCommentByIdAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                Log.Warning("Коментар з ID {CommentId} не знайдено", id);
                return $"Коментар з ID {id} не знайдено.";
            }

            return comment;
        }

        public async Task<Result<Comment>> AddCommentAsync(CreateCommentDto dto, string userId)
        {
            Log.Information("Початок додавання коментаря до поста {PostId}", dto.PostId);

            var comment = new Comment
            {
                UserId = userId,
                PostId = dto.PostId,
                Content = dto.Content,
                Stars = dto.Stars,
            };

            await _commentRepository.AddAsync(comment);

            Log.Information("Коментар успішно додано до поста {PostId}. ID коментаря: {CommentId}", dto.PostId, comment.CommentId)
            var post = _postRepository.GetById(dto.PostId);
            if (post != null)
            {
                var allComments = _commentRepository.GetByPostId(dto.PostId);
                if (allComments.Any())
                {
                    double average = allComments.Average(c => c.Stars);
                    post.Rating = (decimal)average;
                    _postRepository.Update(post);
                }
            }
            return comment;
        }

        public async Task<Result> DeleteCommentAsync(int id)
        {
            Log.Information("Видалення коментаря з ID {CommentId}", id);

            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                Log.Warning("Невдала спроба видалення: коментар з ID {CommentId} не знайдено", id);

                return "Коментар для видалення не знайдено.";
            }

            await _commentRepository.DeleteAsync(comment);

            var post = _postRepository.GetById(postId);
            if (post != null)
            {
                var remainingComments = _commentRepository.GetByPostId(postId);
                post.Rating = remainingComments.Any()
                    ? (decimal)remainingComments.Average(c => c.Stars)
                    : 0;

                _postRepository.Update(post);
            }

            return Result.Success();
        }
    }
}