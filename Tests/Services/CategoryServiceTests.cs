using System.Collections.Generic;
using System.Threading.Tasks;
using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IConfiguration> _configMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            Log.Logger = Serilog.Core.Logger.None;
            _categoryRepoMock = new Mock<ICategoryRepository>();

            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _configMock = new Mock<IConfiguration>();
            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(s => s.Value).Returns("30");
            _configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(sectionMock.Object);

            _categoryService = new CategoryService(
                _categoryRepoMock.Object,
                _memoryCache,
                _configMock.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_WhenRecordsExist_ShouldReturnAll()
        {
            var categories = new List<Category> { new Category { CategoryId = 1 }, new Category { CategoryId = 2 } };
            _categoryRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(categories);

            var result = await _categoryService.GetAllCategoriesAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            _categoryRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Category>());

            var result = await _categoryService.GetAllCategoriesAsync();

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldCallRepositoryOnce()
        {
            _categoryRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Category>());

            await _categoryService.GetAllCategoriesAsync();
            _categoryRepoMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ExistingId_ShouldReturnCategory()
        {
            int catId = 1;
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(catId)).ReturnsAsync(new Category { CategoryId = catId, Name = "Art" });

            var result = await _categoryService.GetCategoryByIdAsync(catId);

            Assert.True(result.IsSuccess);
            Assert.Equal("Art", result.Value.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_NonExistingId_ShouldReturnFailure()
        {
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null!);

            var result = await _categoryService.GetCategoryByIdAsync(999);

            Assert.True(result.IsFailure);
            Assert.Contains("не знайдено", result.ErrorMessage);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldCallGetByIdWithCorrectId()
        {
            int testId = 55;
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(testId)).ReturnsAsync(new Category());

            await _categoryService.GetCategoryByIdAsync(testId);
            _categoryRepoMock.Verify(repo => repo.GetByIdAsync(testId), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_ValidData_ShouldReturnSuccess()
        {
            var dto = new CreateCategoryDto { Name = "Toys" };
            _categoryRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            var result = await _categoryService.CreateCategoryAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Toys", result.Value.Name);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCallAddMethod()
        {
            var dto = new CreateCategoryDto { Name = "Decor" };
            _categoryRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            await _categoryService.CreateCategoryAsync(dto);

            _categoryRepoMock.Verify(repo => repo.AddAsync(It.Is<Category>(c => c.Name == "Decor")), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_WhenDtoHasNullName_ShouldStillCreateEntity()
        {
            var dto = new CreateCategoryDto { Name = null! };
            _categoryRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            var result = await _categoryService.CreateCategoryAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Name);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ExistingCategory_ShouldUpdateAndReturnSuccess()
        {
            int catId = 1;
            var existing = new Category { CategoryId = catId, Name = "Old" };
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(catId)).ReturnsAsync(existing);
            _categoryRepoMock.Setup(repo => repo.UpdateAsync(existing)).Returns(Task.CompletedTask);

            var result = await _categoryService.UpdateCategoryAsync(catId, new UpdateCategoryDto { Name = "New" });

            Assert.True(result.IsSuccess);
            Assert.Equal("New", existing.Name);
            _categoryRepoMock.Verify(repo => repo.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_NonExisting_ShouldReturnFailure()
        {
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null!);

            var result = await _categoryService.UpdateCategoryAsync(1, new UpdateCategoryDto { Name = "Test" });

            Assert.True(result.IsFailure);
            _categoryRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldCallGetByIdBeforeUpdate()
        {
            int catId = 10;
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(catId)).ReturnsAsync(new Category());
            _categoryRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            await _categoryService.UpdateCategoryAsync(catId, new UpdateCategoryDto { Name = "Name" });
            _categoryRepoMock.Verify(repo => repo.GetByIdAsync(catId), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_Existing_ShouldReturnSuccess()
        {
            int catId = 1;
            var category = new Category { CategoryId = catId };
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(catId)).ReturnsAsync(category);
            _categoryRepoMock.Setup(repo => repo.DeleteAsync(category)).Returns(Task.CompletedTask);

            var result = await _categoryService.DeleteCategoryAsync(catId);

            Assert.True(result.IsSuccess);
            _categoryRepoMock.Verify(repo => repo.DeleteAsync(category), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_NonExisting_ShouldReturnFailure()
        {
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null!);

            var result = await _categoryService.DeleteCategoryAsync(1);

            Assert.True(result.IsFailure);
            _categoryRepoMock.Verify(repo => repo.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldCallGetByIdToVerifyExistence()
        {
            int catId = 5;
            _categoryRepoMock.Setup(repo => repo.GetByIdAsync(catId)).ReturnsAsync(new Category { CategoryId = catId });
            _categoryRepoMock.Setup(repo => repo.DeleteAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            await _categoryService.DeleteCategoryAsync(catId);
            _categoryRepoMock.Verify(repo => repo.GetByIdAsync(catId), Times.Once);
        }
    }
}