using System.Collections.Generic;
using System.Threading.Tasks;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _adminService = new AdminService(_userRepoMock.Object);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsersFromRepository()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1" },
                new ApplicationUser { Id = "2" },
            };

            _userRepoMock.Setup(r => r.GetAllExceptAdminsAsync())
                         .ReturnsAsync(users);

            var result = await _adminService.GetUsersAsync();

            Assert.Equal(2, result.Count);
            _userRepoMock.Verify(r => r.GetAllExceptAdminsAsync(), Times.Once);
        }

        [Fact]
        public async Task BlockUserAsync_UserExists_ShouldSetIsBlockedTrue()
        {
            var user = new ApplicationUser { Id = "1", IsBlocked = false };

            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync(user);

            await _adminService.BlockUserAsync("1");

            Assert.True(user.IsBlocked);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task BlockUserAsync_UserNotFound_ShouldDoNothing()
        {
            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync((ApplicationUser)null!);

            await _adminService.BlockUserAsync("1");

            _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task UnblockUserAsync_UserExists_ShouldSetIsBlockedFalse()
        {
            var user = new ApplicationUser { Id = "1", IsBlocked = true };

            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync(user);

            await _adminService.UnblockUserAsync("1");

            Assert.False(user.IsBlocked);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UnblockUserAsync_UserNotFound_ShouldDoNothing()
        {
            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync((ApplicationUser)null!);

            await _adminService.UnblockUserAsync("1");

            _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_UserExists_ShouldCallDelete()
        {
            var user = new ApplicationUser { Id = "1" };

            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync(user);

            await _adminService.DeleteUserAsync("1");

            _userRepoMock.Verify(r => r.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_UserNotFound_ShouldDoNothing()
        {
            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync((ApplicationUser)null!);

            await _adminService.DeleteUserAsync("1");

            _userRepoMock.Verify(r => r.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}