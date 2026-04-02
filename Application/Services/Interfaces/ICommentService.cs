using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface ICommentService
    {
        Task<Result<List<Comment>>> GetCommentsForPostAsync(int postId);

        Task<Result<Comment>> GetCommentByIdAsync(int id);

        Task<Result<Comment>> AddCommentAsync(CreateCommentDto dto, string userId);

        Task<Result> DeleteCommentAsync(int id);
    }
}