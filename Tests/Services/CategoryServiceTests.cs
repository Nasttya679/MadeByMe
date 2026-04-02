using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;
using Serilog;

namespace MadeByMe.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            Log.Logger = Serilog.Core.Logger.None;
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _categoryService = new CategoryService(_categoryRepoMock.Object);
        }

        [Fact]
        public void GetAllCategories_WhenRecordsExist_ShouldReturnAll()
        {
            var categories = new List<Category> { new Category { CategoryId = 1 }, new Category { CategoryId = 2 } };
            _categoryRepoMock.Setup(repo => repo.GetAll()).Returns(categories);

            var result = _categoryService.GetAllCategories();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public void GetAllCategories_WhenEmpty_ShouldReturnEmptyList()
        {
            _categoryRepoMock.Setup(repo => repo.GetAll()).Returns(new List<Category>());

            var result = _categoryService.GetAllCategories();

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public void GetAllCategories_ShouldCallRepositoryOnce()
        {
            _categoryRepoMock.Setup(repo => repo.GetAll()).Returns(new List<Category>());
            _categoryService.GetAllCategories();
            _categoryRepoMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public void GetCategoryById_ExistingId_ShouldReturnCategory()
        {
            int catId = 1;
            _categoryRepoMock.Setup(repo => repo.GetById(catId)).Returns(new Category { CategoryId = catId, Name = "Art" });

            var result = _categoryService.GetCategoryById(catId);

            Assert.True(result.IsSuccess);
            Assert.Equal("Art", result.Value.Name);
        }

        [Fact]
        public void GetCategoryById_NonExistingId_ShouldReturnFailure()
        {
            _categoryRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Category)null!);

            var result = _categoryService.GetCategoryById(999);

            Assert.True(result.IsFailure);
            Assert.Contains("не знайдено", result.ErrorMessage);
        }

        [Fact]
        public void GetCategoryById_ShouldCallGetByIdWithCorrectId()
        {
            int testId = 55;
            _categoryService.GetCategoryById(testId);
            _categoryRepoMock.Verify(repo => repo.GetById(testId), Times.Once);
        }

        [Fact]
        public void CreateCategory_ValidData_ShouldReturnSuccess()
        {
            var dto = new CreateCategoryDto { Name = "Toys" };

            var result = _categoryService.CreateCategory(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Toys", result.Value.Name);
        }

        [Fact]
        public void CreateCategory_ShouldCallAddMethod()
        {
            var dto = new CreateCategoryDto { Name = "Decor" };

            _categoryService.CreateCategory(dto);

            _categoryRepoMock.Verify(repo => repo.Add(It.Is<Category>(c => c.Name == "Decor")), Times.Once);
        }

        [Fact]
        public void CreateCategory_WhenDtoHasNullName_ShouldStillCreateEntity()
        {
            var dto = new CreateCategoryDto { Name = null! };

            var result = _categoryService.CreateCategory(dto);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Name);
        }

        [Fact]
        public void UpdateCategory_ExistingCategory_ShouldUpdateAndReturnSuccess()
        {
            int catId = 1;
            var existing = new Category { CategoryId = catId, Name = "Old" };
            _categoryRepoMock.Setup(repo => repo.GetById(catId)).Returns(existing);

            var result = _categoryService.UpdateCategory(catId, new UpdateCategoryDto { Name = "New" });

            Assert.True(result.IsSuccess);
            Assert.Equal("New", existing.Name);
            _categoryRepoMock.Verify(repo => repo.Update(existing), Times.Once);
        }

        [Fact]
        public void UpdateCategory_NonExisting_ShouldReturnFailure()
        {
            _categoryRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Category)null!);

            var result = _categoryService.UpdateCategory(1, new UpdateCategoryDto { Name = "Test" });

            Assert.True(result.IsFailure);
            _categoryRepoMock.Verify(repo => repo.Update(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void UpdateCategory_ShouldCallGetByIdBeforeUpdate()
        {
            int catId = 10;
            _categoryService.UpdateCategory(catId, new UpdateCategoryDto { Name = "Name" });
            _categoryRepoMock.Verify(repo => repo.GetById(catId), Times.Once);
        }

        [Fact]
        public void DeleteCategory_Existing_ShouldReturnSuccess()
        {
            int catId = 1;
            var category = new Category { CategoryId = catId };
            _categoryRepoMock.Setup(repo => repo.GetById(catId)).Returns(category);

            var result = _categoryService.DeleteCategory(catId);

            Assert.True(result.IsSuccess);
            _categoryRepoMock.Verify(repo => repo.Delete(category), Times.Once);
        }

        [Fact]
        public void DeleteCategory_NonExisting_ShouldReturnFailure()
        {
            _categoryRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Category)null!);

            var result = _categoryService.DeleteCategory(1);

            Assert.True(result.IsFailure);
            _categoryRepoMock.Verify(repo => repo.Delete(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void DeleteCategory_ShouldCallGetByIdToVerifyExistence()
        {
            int catId = 5;
            _categoryService.DeleteCategory(catId);
            _categoryRepoMock.Verify(repo => repo.GetById(catId), Times.Once);
        }
    }
}