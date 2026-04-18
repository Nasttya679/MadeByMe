using System.IO;
using System.Text;
using System.Threading.Tasks;
using MadeByMe.Application.Common;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class PhotoServiceTests
    {
        private readonly Mock<IPhotoRepository> _photoRepoMock;
        private readonly Mock<IOptions<ProjectSettings>> _optionsMock;
        private readonly PhotoService _photoService;

        public PhotoServiceTests()
        {
            _photoRepoMock = new Mock<IPhotoRepository>();
            _optionsMock = new Mock<IOptions<ProjectSettings>>();

            var settings = new ProjectSettings();
            settings.FileStorage.UploadFolder = "test_images";
            settings.FileStorage.DefaultImagePath = "/images/default.jpg";
            settings.FileStorage.MaxImageSizeMB = 5;
            settings.FileStorage.AllowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            _optionsMock.Setup(o => o.Value).Returns(settings);

            _photoService = new PhotoService(_photoRepoMock.Object, _optionsMock.Object);
        }

        [Fact]
        public async Task SavePhotoAsync_WhenFileIsNull_ShouldReturnFailure()
        {
            var result = await _photoService.SavePhotoAsync(null!);

            Assert.True(result.IsFailure);
            Assert.Equal("Файл не завантажено або він порожній.", result.ErrorMessage);
        }

        [Fact]
        public async Task SavePhotoAsync_WhenFileIsEmpty_ShouldReturnFailure()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            var result = await _photoService.SavePhotoAsync(fileMock.Object);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task SavePhotoAsync_WithValidFile_ShouldReturnPhotoObject()
        {
            var content = "fake image content";
            var fileName = "test.jpg";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

            var result = await _photoService.SavePhotoAsync(fileMock.Object, 10);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.FileName);
            Assert.Equal(10, result.Value.PostId);
            Assert.Contains("/images/", result.Value.FilePath);

            _photoRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Photo>()), Times.Once);
        }

        [Fact]
        public async Task DeletePhotoAsync_WhenPhotoIsNull_ShouldReturnFailure()
        {
            var result = await _photoService.DeletePhotoAsync(null!);

            Assert.True(result.IsFailure);
            Assert.Contains("порожнє", result.ErrorMessage);
        }

        [Fact]
        public async Task DeletePhotoAsync_WhenPhotoExists_ShouldReturnSuccess()
        {
            var photo = new Photo { FileName = "non-existent-file.jpg" };

            var result = await _photoService.DeletePhotoAsync(photo);

            Assert.True(result.IsSuccess);

            _photoRepoMock.Verify(repo => repo.DeleteAsync(photo), Times.Once);
        }

        [Fact]
        public async Task DeletePhotoAsync_ShouldHandleNullFileNameGracefully()
        {
            var photo = new Photo { FileName = null };

            var result = await _photoService.DeletePhotoAsync(photo);

            Assert.True(result.IsFailure);
            Assert.Contains("порожнє", result.ErrorMessage);
        }

        [Fact]
        public void GetPhotoUrl_WhenPhotoIsNull_ShouldReturnDefaultPath()
        {
            var url = _photoService.GetPhotoUrl(null!);

            Assert.Equal("/images/default.jpg", url);
        }

        [Fact]
        public void GetPhotoUrl_WhenFilePathExists_ShouldReturnCorrectUrl()
        {
            var photo = new Photo { FilePath = "/images/my-cool-photo.png" };

            var url = _photoService.GetPhotoUrl(photo);

            Assert.Equal("/images/my-cool-photo.png", url);
        }

        [Fact]
        public void GetPhotoUrl_WhenFilePathIsNull_ShouldReturnDefaultPath()
        {
            var photo = new Photo { FilePath = null };

            var url = _photoService.GetPhotoUrl(photo);

            Assert.Equal("/images/default.jpg", url);
        }
    }
}