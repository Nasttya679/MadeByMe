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

        public Result<List<Comment>> GetCommentsForPost(int postId)
        {
            var comments = _commentRepository.GetByPostId(postId);
            Log.Information("Отримано список коментарів для поста {PostId}. Кількість: {Count}", postId, comments.Count);
            return Result<List<Comment>>.Success(comments);
        }

        public Result<Comment> GetCommentById(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                return Result<Comment>.Failure($"Коментар з ID {id} не знайдено.");
            }

            return Result<Comment>.Success(comment);
        }

        public Result<Comment> AddComment(CreateCommentDto dto, string userId)
        {
            Log.Information("Початок додавання коментаря до поста {PostId}", dto.PostId);

            var comment = new Comment
            {
                UserId = userId,
                PostId = dto.PostId,
                Content = dto.Content,
                Stars = dto.Stars,
            };

            _commentRepository.Add(comment);

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

            Log.Information("Коментар успішно додано. Оновлено рейтинг поста.");
            return Result<Comment>.Success(comment);
        }

        public Result DeleteComment(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                return Result.Failure("Коментар не знайдено.");
            }

            int postId = comment.PostId;
            _commentRepository.Delete(comment);

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