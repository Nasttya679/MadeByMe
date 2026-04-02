using System.Threading.Tasks;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class ApplicationUserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly ApplicationUserService _userService;

        public ApplicationUserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();

            _userService = new ApplicationUserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserNotFound_ShouldReturnFailure()
        {
            string userId = "invalid-id";
            var dto = new UpdateProfileDto();

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                               .ReturnsAsync((ApplicationUser)null!);

            var result = await _userService.UpdateUserAsync(userId, dto);

            Assert.True(result.IsFailure);
            Assert.Equal("Користувача з таким ідентифікатором не знайдено.", result.ErrorMessage);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidFullData_ShouldUpdateAllFieldsAndReturnSuccess()
        {
            string userId = "valid-id";
            var existingUser = new ApplicationUser
            {
                Id = userId,
                UserName = "OldName",
                Email = "old@email.com",
                ProfilePicture = "old_pic.jpg",
                PhoneNumber = "12345",
                Address = "Old Address",
            };

            var dto = new UpdateProfileDto
            {
                UserName = "NewName",
                Email = "new@email.com",
                ProfilePicture = "new_pic.jpg",
                PhoneNumber = "98765",
                Address = "New Address",
            };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                               .ReturnsAsync(existingUser);

            var result = await _userService.UpdateUserAsync(userId, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("NewName", result.Value.UserName);
            Assert.Equal("new@email.com", result.Value.Email);
            Assert.Equal("New Address", result.Value.Address);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithPartialNullData_ShouldKeepOldValuesAndReturnSuccess()
        {
            string userId = "valid-id";
            var existingUser = new ApplicationUser
            {
                Id = userId,
                UserName = "OldName",
                Email = "old@email.com",
            };

            var dto = new UpdateProfileDto
            {
                UserName = "NewName",
                Email = null,
                ProfilePicture = null,
                PhoneNumber = null,
                Address = null,
            };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                               .ReturnsAsync(existingUser);

            var result = await _userService.UpdateUserAsync(userId, dto);

            Assert.True(result.IsSuccess);

            Assert.Equal("NewName", result.Value.UserName);

            Assert.Equal("old@email.com", result.Value.Email);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
        }
    }
}