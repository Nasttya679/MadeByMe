using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IPhotoRepository
    {
        Task AddAsync(Photo photo);

        Task DeleteAsync(Photo photo);

        Task<Photo?> GetByIdAsync(int id);
    }
}