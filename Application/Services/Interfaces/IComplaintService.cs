using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IComplaintService
    {
        Task<Result> SubmitComplaintAsync(string reporterId, ComplaintDto dto);

        Task<Result<List<Complaint>>> GetPendingComplaintsAsync();

        Task<Result> RejectComplaintAsync(int complaintId);

        Task<Result> ApproveAndPunishAsync(int complaintId);
    }
}