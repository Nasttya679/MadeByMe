using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Comment> GetByPostId(int postId)
        {
            return _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .ToList();
        }

        public Comment? GetById(int id)
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefault(c => c.CommentId == id);
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }

        public void Delete(Comment comment)
        {
            _context.Comments.Remove(comment);
            _context.SaveChanges();
        }
    }
}