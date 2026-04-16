using System.Collections.Generic;
using System.Threading.Tasks;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _cartRepoMock;
        private readonly Mock<IBuyerCartRepository> _buyerCartRepoMock;
        private readonly Mock<IPostRepository> _postRepoMock;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _cartRepoMock = new Mock<ICartRepository>();
            _buyerCartRepoMock = new Mock<IBuyerCartRepository>();
            _postRepoMock = new Mock<IPostRepository>();

            _cartService = new CartService(
                _cartRepoMock.Object,
                _buyerCartRepoMock.Object,
                _postRepoMock.Object);
        }

        [Fact]
        public async Task GetUserCartEntityAsync_WhenCartExists_ShouldReturnSuccessWithCart()
        {
            string buyerId = "user123";
            var expectedCart = new Cart { CartId = 1, BuyerId = buyerId };
            _cartRepoMock.Setup(repo => repo.GetCartByBuyerIdAsync(buyerId)).ReturnsAsync(expectedCart);

            var result = await _cartService.GetUserCartEntityAsync(buyerId);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedCart, result.Value);
        }

        [Fact]
        public async Task GetUserCartEntityAsync_WhenCartDoesNotExist_ShouldReturnFailure()
        {
            string buyerId = "unknown";
            _cartRepoMock.Setup(repo => repo.GetCartByBuyerIdAsync(buyerId)).ReturnsAsync((Cart)null!);

            var result = await _cartService.GetUserCartEntityAsync(buyerId);

            Assert.True(result.IsFailure);
            Assert.Equal("Кошик користувача не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public async Task GetCartTotalAsync_WhenCartIsEmpty_ShouldReturnZero()
        {
            int cartId = 1;
            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartIdAsync(cartId)).ReturnsAsync(new List<BuyerCart>());

            var result = await _cartService.GetCartTotalAsync(cartId);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Value);
        }

        [Fact]
        public async Task GetCartTotalAsync_WhenCartHasItems_ShouldCalculateCorrectSum()
        {
            int cartId = 1;
            var items = new List<BuyerCart>
            {
                new BuyerCart { PostId = 10, Quantity = 2 },
                new BuyerCart { PostId = 20, Quantity = 1 },
            };

            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartIdAsync(cartId)).ReturnsAsync(items);
            _postRepoMock.Setup(repo => repo.GetByIdAsync(10)).ReturnsAsync(new Post { Id = 10, Price = 100 });
            _postRepoMock.Setup(repo => repo.GetByIdAsync(20)).ReturnsAsync(new Post { Id = 20, Price = 50 });

            var result = await _cartService.GetCartTotalAsync(cartId);

            Assert.True(result.IsSuccess);
            Assert.Equal(250, result.Value);
        }

        [Fact]
        public async Task GetCartTotalAsync_WhenProductNotFound_ShouldReturnFailure()
        {
            int cartId = 1;
            var items = new List<BuyerCart> { new BuyerCart { PostId = 999, Quantity = 1 } };

            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartIdAsync(cartId)).ReturnsAsync(items);
            _postRepoMock.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Post)null!);

            var result = await _cartService.GetCartTotalAsync(cartId);

            Assert.True(result.IsFailure);
            Assert.Contains("товар з ID 999 не знайдено", result.ErrorMessage);
        }

        [Fact]
        public async Task ClearCartAsync_WhenCartHasItems_ShouldCallRemoveRange()
        {
            int cartId = 1;
            var items = new List<BuyerCart> { new BuyerCart { CartId = 1 } };
            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartIdAsync(cartId)).ReturnsAsync(items);

            var result = await _cartService.ClearCartAsync(cartId);

            Assert.True(result.IsSuccess);
            _buyerCartRepoMock.Verify(repo => repo.RemoveRangeAsync(items), Times.Once);
        }

        [Fact]
        public async Task ClearCartAsync_WhenCartAlreadyEmpty_ShouldNotCallRemoveRange()
        {
            int cartId = 1;
            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartIdAsync(cartId)).ReturnsAsync(new List<BuyerCart>());

            var result = await _cartService.ClearCartAsync(cartId);

            Assert.True(result.IsSuccess);
            _buyerCartRepoMock.Verify(repo => repo.RemoveRangeAsync(It.IsAny<IEnumerable<BuyerCart>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCartItemAsync_WhenDataIsValid_ShouldCallUpdate()
        {
            var item = new BuyerCart { CartId = 1, PostId = 1, Quantity = 5 };

            var result = await _cartService.UpdateCartItemAsync(item);

            Assert.True(result.IsSuccess);
            _buyerCartRepoMock.Verify(repo => repo.UpdateItemAsync(item), Times.Once);
        }

        [Fact]
        public async Task UpdateCartItemAsync_WhenItemIsNull_ShouldReturnFailure()
        {
            var result = await _cartService.UpdateCartItemAsync(null!);

            Assert.True(result.IsFailure);
            _buyerCartRepoMock.Verify(repo => repo.UpdateItemAsync(It.IsAny<BuyerCart>()), Times.Never);
        }
    }
}