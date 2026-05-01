using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MadeByMe.Application.Services.Implementations
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepo;

        public ComplaintService(IComplaintRepository complaintRepo)
        {
            _complaintRepo = complaintRepo;
        }

        public async Task<Result> SubmitComplaintAsync(string reporterId, ComplaintDto dto)
        {
            var complaint = new Complaint
            {
                ReporterId = reporterId,
                PostId = dto.PostId,
                SellerId = dto.SellerId,
                Reason = dto.Reason,
                Description = dto.Description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
            };

            await _complaintRepo.CreateComplaintAsync(complaint);

            Log.Information("Користувач {ReporterId} створив скаргу на Пост: {PostId}, Продавця: {SellerId}", reporterId, dto.PostId, dto.SellerId);

            return Result.Success();
        }

        public async Task<Result<List<Complaint>>> GetPendingComplaintsAsync()
        {
            return await _complaintRepo.GetPendingComplaintsAsync();
        }

        public async Task<Result> RejectComplaintAsync(int complaintId)
        {
            var complaint = await _complaintRepo.GetComplaintWithDetailsAsync(complaintId);
            if (complaint == null)
            {
                return "Скаргу не знайдено.";
            }

            complaint.Status = "Rejected";
            await _complaintRepo.UpdateComplaintAsync(complaint);

            Log.Information("Скарга {ComplaintId} була відхилена адміном.", complaintId);
            return Result.Success();
        }

        public async Task<Result> ApproveAndPunishAsync(int complaintId)
        {
            var complaint = await _complaintRepo.GetComplaintWithDetailsAsync(complaintId);

            if (complaint == null)
            {
                return "Скаргу не знайдено.";
            }

            if (complaint.Status != "Pending")
            {
                return "Ця скарга вже оброблена.";
            }

            complaint.Status = "Approved";

            if (complaint.PostId != null && complaint.Post != null)
            {
                complaint.Post.Rating -= 1.0m;

                if (complaint.Post.Rating <= 0)
                {
                    complaint.Post.Rating = 0;
                    complaint.Post.IsDeleted = true;
                    Log.Warning("Пост {PostId} автоматично видалено через низький рейтинг.", complaint.Post.Id);
                }
            }

            await _complaintRepo.UpdateComplaintAsync(complaint);

            var targetSellerId = complaint.SellerId ?? complaint.Post?.SellerId;

            if (targetSellerId != null)
            {
                var violationsCount = await _complaintRepo.GetSellerViolationsCountAsync(targetSellerId);

                if (violationsCount >= 3)
                {
                    await _complaintRepo.BlockSellerAsync(targetSellerId);
                    Log.Warning("Продавця {SellerId} АВТОМАТИЧНО ЗАБЛОКОВАНО за систематичні порушення!", targetSellerId);
                }
            }

            Log.Information("Скарга {ComplaintId} успішно схвалена, покарання застосовано.", complaintId);
            return Result.Success();
        }
    }
}