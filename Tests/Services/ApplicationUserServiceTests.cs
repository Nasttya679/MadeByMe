using System.Threading.Tasks;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class ApplicationUserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly ApplicationUserService _userService;

        public ApplicationUserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();

            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _userService = new ApplicationUserService(_userRepositoryMock.Object, _userManagerMock.Object);
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

        [Fact]
        public async Task GetSellerProfileAsync_WhenUserExists_ShouldReturnUser()
        {
            var userId = "existing-seller";
            var expectedUser = new ApplicationUser { Id = userId, UserName = "CraftMaster" };

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            var result = await _userService.GetSellerProfileAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.Equal("CraftMaster", result.Value.UserName);
        }

        [Fact]
        public async Task GetSellerProfileAsync_WhenUserNotFound_ShouldReturnFailure()
        {
            var userId = "missing-seller";

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null!);

            var result = await _userService.GetSellerProfileAsync(userId);

            Assert.True(result.IsFailure);
            Assert.Equal("Користувача (продавця) не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateSellerDescriptionAsync_WhenUserExists_ShouldUpdateAndReturnSuccess()
        {
            var userId = "seller-123";
            var newDescription = "Новий крутий опис мого магазину";
            var user = new ApplicationUser { Id = userId, SellerDescription = "Старий опис" };

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);

            var result = await _userService.UpdateSellerDescriptionAsync(userId, newDescription);

            Assert.True(result.IsSuccess);
            Assert.Equal(newDescription, user.SellerDescription);
            _userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateSellerDescriptionAsync_WhenUserNotFound_ShouldReturnFailureAndNotCallUpdate()
        {
            var userId = "missing-user";

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null!);

            var result = await _userService.UpdateSellerDescriptionAsync(userId, "Якийсь опис");

            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
            _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}