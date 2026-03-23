using MadeByMe.Domain.Entities;
using MadeByMe.Application.DTOs;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using MadeByMe.Application.Services.Interfaces;
using System.Collections.Generic;
using MadeByMe.Application.Services.Interfaces;

namespace MadeByMe.Application.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public List<Comment> GetCommentsForPost(int postId)
        {
            return _commentRepository.GetByPostId(postId);
        }

        public Comment GetCommentById(int id)
        {
            return _commentRepository.GetById(id)!;
        }

        public Comment AddComment(CreateCommentDto dto, string userId)
        {
            var comment = new Comment
            {
                UserId = userId,
                PostId = dto.PostId,
                Content = dto.Content
            };

            _commentRepository.Add(comment);
            return comment;
        }

        public bool DeleteComment(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment != null)
            {
                _commentRepository.Delete(comment);
                return true;
            }
            return false;
        }
    }
}