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

        public async Task<List<Post>> GetAllAsync() =>
            await _context.Posts.Include(p => p.Category)
                                .Include(p => p.Seller)
                                .Include(p => p.Photos)
                                .ToListAsync();

        public async Task<Post?> GetByIdAsync(int id) =>
            await _context.Posts.Include(p => p.Category)
                                .Include(p => p.Seller)
                                .Include(p => p.CommentsList)
                                .Include(p => p.Photos)
                                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Post post)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}