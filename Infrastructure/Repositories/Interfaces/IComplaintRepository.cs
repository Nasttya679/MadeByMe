using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface IComplaintRepository
    {
        Task CreateComplaintAsync(Complaint complaint);

        Task<List<Complaint>> GetPendingComplaintsAsync();

        Task<Complaint?> GetComplaintWithDetailsAsync(int id);

        Task UpdateComplaintAsync(Complaint complaint);

        Task<int> GetSellerViolationsCountAsync(string sellerId);

        Task BlockSellerAsync(string sellerId);
    }
}