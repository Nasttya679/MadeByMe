using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(int id);

        Task<List<Comment>> GetByPostIdAsync(int postId);

        Task AddAsync(Comment comment);

        Task DeleteAsync(Comment comment);

        Task<int> GetCountBySellerIdAsync(string sellerId);

        Task<int> GetCountByUserIdAsync(string userId);
    }
}