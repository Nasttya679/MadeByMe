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
        public async Task GetUsersAsync_ShouldReturnUsersFromRepository_AsSuccessResult()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1" },
                new ApplicationUser { Id = "2" },
            };

            _userRepoMock.Setup(r => r.GetAllExceptAdminsAsync())
                         .ReturnsAsync(users);

            // Act
            var result = await _adminService.GetUsersAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            _userRepoMock.Verify(r => r.GetAllExceptAdminsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUsersAsync_WhenNoUsersExist_ShouldReturnSuccessWithEmptyList()
        {
            // Arrange
            var emptyList = new List<ApplicationUser>();

            _userRepoMock.Setup(r => r.GetAllExceptAdminsAsync())
                         .ReturnsAsync(emptyList);

            // Act
            var result = await _adminService.GetUsersAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
            _userRepoMock.Verify(r => r.GetAllExceptAdminsAsync(), Times.Once);
        }

        [Fact]
        public async Task BlockUserAsync_UserExists_ShouldSetIsBlockedTrue_AndReturnSuccess()
        {
            var user = new ApplicationUser { Id = "1", IsBlocked = false };

            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync(user);

            var result = await _adminService.BlockUserAsync("1");

            Assert.True(result.IsSuccess);
            Assert.True(user.IsBlocked);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task BlockUserAsync_UserNotFound_ShouldReturnFailure()
        {
            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync((ApplicationUser)null!);

            var result = await _adminService.BlockUserAsync("1");

            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
            _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task UnblockUserAsync_UserExists_ShouldSetIsBlockedFalse_AndReturnSuccess()
        {
            var user = new ApplicationUser { Id = "1", IsBlocked = true };

            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync(user);

            var result = await _adminService.UnblockUserAsync("1");

            Assert.True(result.IsSuccess);
            Assert.False(user.IsBlocked);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UnblockUserAsync_UserNotFound_ShouldReturnFailure()
        {
            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync((ApplicationUser)null!);

            var result = await _adminService.UnblockUserAsync("1");

            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
            _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_UserExists_ShouldCallDelete_AndReturnSuccess()
        {
            var user = new ApplicationUser { Id = "1" };

            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync(user);

            var result = await _adminService.DeleteUserAsync("1");

            Assert.True(result.IsSuccess);
            _userRepoMock.Verify(r => r.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_UserNotFound_ShouldReturnFailure()
        {
            _userRepoMock.Setup(r => r.GetByIdAsync("1"))
                         .ReturnsAsync((ApplicationUser)null!);

            var result = await _adminService.DeleteUserAsync("1");

            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
            _userRepoMock.Verify(r => r.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}