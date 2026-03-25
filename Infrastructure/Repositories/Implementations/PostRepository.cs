using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context) => _context = context;

        public List<Post> GetAll() => _context.Posts.Include(p => p.Category)
                                                .Include(p => p.Seller)
                                                .Include(p => p.Photos)
                                                .ToList();

        public Post? GetById(int id) => _context.Posts
                                            .Include(p => p.Category)
                                            .Include(p => p.Seller)
                                            .Include(p => p.CommentsList)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault(p => p.Id == id);

        public void Add(Post post)
        {
            _context.Posts.Add(post);
            _context.SaveChanges();
        }

        public void Update(Post post)
        {
            _context.Posts.Update(post);
            _context.SaveChanges();
        }

        public void Delete(Post post)
        {
            _context.Posts.Remove(post);
            _context.SaveChanges();
        }
    }
}