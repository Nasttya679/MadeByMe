using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IPhotoRepository
    {
        void Add(Photo photo);

        void Delete(Photo photo);

        Photo? GetById(int id);
    }
}