using MadeByMe.Domain.Entities;
using MadeByMe.Application.DTOs;
using System.Collections.Generic;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface ICommentService
    {
        List<Comment> GetCommentsForPost(int postId);
        Comment GetCommentById(int id);
        Comment AddComment(CreateCommentDto dto, string userId);
        bool DeleteComment(int id);
    }
}