using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;

namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly ApplicationDbContext _context;

        public PhotoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Photo photo)
        {
            _context.Photos.Add(photo);
            _context.SaveChanges();
        }

        public void Delete(Photo photo)
        {
            _context.Photos.Remove(photo);
            _context.SaveChanges();
        }

        public Photo? GetById(int id)
        {
            return _context.Photos.Find(id);
        }
    }
}