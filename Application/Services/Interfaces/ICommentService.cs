using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface ICommentService
    {
        Result<List<Comment>> GetCommentsForPost(int postId);

        Result<Comment> GetCommentById(int id);

        Result<Comment> AddComment(CreateCommentDto dto, string userId);

        Result DeleteComment(int id);
    }
}