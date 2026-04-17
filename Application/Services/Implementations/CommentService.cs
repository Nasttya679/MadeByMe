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

            Log.Information("Коментар успішно додано до поста {PostId}. ID коментаря: {CommentId}", dto.PostId, comment.CommentId);

            var post = await _postRepository.GetByIdAsync(dto.PostId);
            if (post != null)
            {
                var allComments = await _commentRepository.GetByPostIdAsync(dto.PostId);
                if (allComments.Any())
                {
                    double average = allComments.Average(c => c.Stars);
                    post.Rating = (decimal)average;
                    await _postRepository.UpdateAsync(post);
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

            int postId = comment.PostId;
            await _commentRepository.DeleteAsync(comment);

            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                var remainingComments = await _commentRepository.GetByPostIdAsync(postId);
                post.Rating = remainingComments.Any()
                    ? (decimal)remainingComments.Average(c => c.Stars)
                    : 0;

                await _postRepository.UpdateAsync(post);
            }

            return Result.Success();
        }

        public async Task<Result<int>> GetSellerReviewsCountAsync(string sellerId)
        {
            var count = await _commentRepository.GetCountBySellerIdAsync(sellerId);
            return count;
        }

        public async Task<Result<int>> GetUserReviewsCountAsync(string userId)
        {
            var count = await _commentRepository.GetCountByUserIdAsync(userId);
            return count;
        }
    }
}