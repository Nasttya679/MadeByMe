using System.Collections.Generic;
using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;

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
            var comment = new Comment
            {
                UserId = userId,
                PostId = dto.PostId,
                Content = dto.Content,
            };

            _commentRepository.Add(comment);
            return Result<Comment>.Success(comment);
        }

        public Result DeleteComment(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                return Result.Failure("Коментар для видалення не знайдено.");
            }

            _commentRepository.Delete(comment);
            return Result.Success();
        }
    }
}