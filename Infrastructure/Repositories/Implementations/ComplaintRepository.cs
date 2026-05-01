using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Data;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MadeByMe.Infrastructure.Repositories.Implementations
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly ApplicationDbContext _context;

        public ComplaintRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateComplaintAsync(Complaint complaint)
        {
            await _context.Complaints.AddAsync(complaint);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Complaint>> GetPendingComplaintsAsync()
        {
            return await _context.Complaints
                .Include(c => c.Reporter)
                .Include(c => c.Post)
                .Include(c => c.Seller)
                .Where(c => c.Status == "Pending")
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Complaint?> GetComplaintWithDetailsAsync(int id)
        {
            return await _context.Complaints
                .Include(c => c.Post)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateComplaintAsync(Complaint complaint)
        {
            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetSellerViolationsCountAsync(string sellerId)
        {
            return await _context.Complaints
                .CountAsync(c => (c.SellerId == sellerId || c.Post!.SellerId == sellerId)
                                 && c.Status == "Approved");
        }

        public async Task BlockSellerAsync(string sellerId)
        {
            var seller = await _context.Users.FindAsync(sellerId);
            if (seller != null)
            {
                seller.IsBlocked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}