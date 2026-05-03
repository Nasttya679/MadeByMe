using System.Collections.Generic;
using System.Threading.Tasks;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class ComplaintServiceTests
    {
        private readonly Mock<IComplaintRepository> _complaintRepoMock;
        private readonly ComplaintService _complaintService;

        public ComplaintServiceTests()
        {
            _complaintRepoMock = new Mock<IComplaintRepository>();
            _complaintService = new ComplaintService(_complaintRepoMock.Object);
        }

        [Fact]
        public async Task SubmitComplaintAsync_WhenValidData_ShouldCallCreateAndReturnSuccess()
        {
            string reporterId = "user1";
            var dto = new ComplaintDto { PostId = 1, SellerId = "seller1", Reason = "Спам", Description = "Реклама казино" };

            var result = await _complaintService.SubmitComplaintAsync(reporterId, dto);

            Assert.True(result.IsSuccess);
            _complaintRepoMock.Verify(repo => repo.CreateComplaintAsync(It.IsAny<Complaint>()), Times.Once);
        }

        [Fact]
        public async Task SubmitComplaintAsync_ShouldSetInitialStatusToPending()
        {
            var dto = new ComplaintDto { PostId = 1, Reason = "Спам" };
            Complaint savedComplaint = null!;

            _complaintRepoMock.Setup(repo => repo.CreateComplaintAsync(It.IsAny<Complaint>()))
                .Callback<Complaint>(c => savedComplaint = c)
                .Returns(Task.CompletedTask);

            await _complaintService.SubmitComplaintAsync("reporter1", dto);

            Assert.NotNull(savedComplaint);
            Assert.Equal("Pending", savedComplaint.Status);
        }

        [Fact]
        public async Task GetPendingComplaintsAsync_WhenComplaintsExist_ShouldReturnList()
        {
            var pendingList = new List<Complaint>
            {
                new Complaint { Id = 1, Status = "Pending" },
                new Complaint { Id = 2, Status = "Pending" },
            };

            _complaintRepoMock.Setup(repo => repo.GetPendingComplaintsAsync())
                .ReturnsAsync(pendingList);

            var result = await _complaintService.GetPendingComplaintsAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task GetPendingComplaintsAsync_WhenNoComplaints_ShouldReturnEmptyList()
        {
            _complaintRepoMock.Setup(repo => repo.GetPendingComplaintsAsync())
                .ReturnsAsync(new List<Complaint>());

            var result = await _complaintService.GetPendingComplaintsAsync();

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task RejectComplaintAsync_WhenComplaintDoesNotExist_ShouldReturnFailure()
        {
            _complaintRepoMock.Setup(repo => repo.GetComplaintWithDetailsAsync(99))
                .ReturnsAsync((Complaint)null!);

            var result = await _complaintService.RejectComplaintAsync(99);

            Assert.True(result.IsFailure);
            Assert.Equal("Скаргу не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public async Task RejectComplaintAsync_WhenComplaintExists_ShouldUpdateStatusToRejected()
        {
            var complaint = new Complaint { Id = 1, Status = "Pending" };
            _complaintRepoMock.Setup(repo => repo.GetComplaintWithDetailsAsync(1))
                .ReturnsAsync(complaint);

            var result = await _complaintService.RejectComplaintAsync(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Rejected", complaint.Status);
            _complaintRepoMock.Verify(repo => repo.UpdateComplaintAsync(complaint), Times.Once);
        }

        [Fact]
        public async Task ApproveAndPunishAsync_WhenNotPending_ShouldReturnFailure()
        {
            var complaint = new Complaint { Id = 1, Status = "Rejected" };
            _complaintRepoMock.Setup(repo => repo.GetComplaintWithDetailsAsync(1))
                .ReturnsAsync(complaint);

            var result = await _complaintService.ApproveAndPunishAsync(1);

            Assert.True(result.IsFailure);
            Assert.Equal("Ця скарга вже оброблена.", result.ErrorMessage);
        }

        [Fact]
        public async Task ApproveAndPunishAsync_WhenValid_ShouldDecreaseRatingAndApprove()
        {
            var post = new Post { Id = 10, Rating = 5.0m, SellerId = "seller1" };
            var complaint = new Complaint { Id = 1, Status = "Pending", PostId = 10, Post = post, SellerId = "seller1" };

            _complaintRepoMock.Setup(repo => repo.GetComplaintWithDetailsAsync(1)).ReturnsAsync(complaint);
            _complaintRepoMock.Setup(repo => repo.GetSellerViolationsCountAsync("seller1")).ReturnsAsync(1);

            var result = await _complaintService.ApproveAndPunishAsync(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Approved", complaint.Status);
            Assert.Equal(4.0m, post.Rating);
            Assert.False(post.IsDeleted);
            _complaintRepoMock.Verify(repo => repo.UpdateComplaintAsync(complaint), Times.Once);
        }

        [Fact]
        public async Task ApproveAndPunishAsync_WhenSellerHas3Violations_ShouldBlockSeller()
        {
            var post = new Post { Id = 10, Rating = 5.0m };
            var complaint = new Complaint { Id = 1, Status = "Pending", PostId = 10, Post = post, SellerId = "bad_seller" };

            _complaintRepoMock.Setup(repo => repo.GetComplaintWithDetailsAsync(1)).ReturnsAsync(complaint);
            _complaintRepoMock.Setup(repo => repo.GetSellerViolationsCountAsync("bad_seller")).ReturnsAsync(3);

            var result = await _complaintService.ApproveAndPunishAsync(1);

            Assert.True(result.IsSuccess);
            _complaintRepoMock.Verify(repo => repo.BlockSellerAsync("bad_seller"), Times.Once);
        }

        [Fact]
        public async Task ApproveAndPunishAsync_WhenRatingDropsToZero_ShouldMarkPostAsDeleted()
        {
            var post = new Post { Id = 10, Rating = 0.5m };
            var complaint = new Complaint { Id = 1, Status = "Pending", PostId = 10, Post = post };

            _complaintRepoMock.Setup(repo => repo.GetComplaintWithDetailsAsync(1)).ReturnsAsync(complaint);
            _complaintRepoMock.Setup(repo => repo.GetSellerViolationsCountAsync(It.IsAny<string>())).ReturnsAsync(0);

            await _complaintService.ApproveAndPunishAsync(1);

            Assert.Equal(0, post.Rating);
            Assert.True(post.IsDeleted);
        }
    }
}