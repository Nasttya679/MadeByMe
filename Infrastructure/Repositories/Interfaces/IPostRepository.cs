using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllAsync();

        Task<Post?> GetByIdAsync(int id);

        Task AddAsync(Post post);

        Task UpdateAsync(Post post);

        Task DeleteAsync(Post post);
    }
}