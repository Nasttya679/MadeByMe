using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;

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
        public void GetUserCartEntity_WhenCartExists_ShouldReturnSuccessWithCart()
        {
            string buyerId = "user123";
            var expectedCart = new Cart { CartId = 1, BuyerId = buyerId };
            _cartRepoMock.Setup(repo => repo.GetCartByBuyerId(buyerId)).Returns(expectedCart);

            var result = _cartService.GetUserCartEntity(buyerId);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedCart, result.Value);
        }

        [Fact]
        public void GetUserCartEntity_WhenCartDoesNotExist_ShouldReturnFailure()
        {
            string buyerId = "unknown";
            _cartRepoMock.Setup(repo => repo.GetCartByBuyerId(buyerId)).Returns((Cart)null!);

            var result = _cartService.GetUserCartEntity(buyerId);

            Assert.True(result.IsFailure);
            Assert.Equal("Кошик користувача не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public void GetCartTotal_WhenCartIsEmpty_ShouldReturnZero()
        {
            int cartId = 1;
            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartId(cartId)).Returns(new List<BuyerCart>());

            var result = _cartService.GetCartTotal(cartId);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Value);
        }

        [Fact]
        public void GetCartTotal_WhenCartHasItems_ShouldCalculateCorrectSum()
        {
            int cartId = 1;
            var items = new List<BuyerCart>
            {
                new BuyerCart { PostId = 10, Quantity = 2 },
                new BuyerCart { PostId = 20, Quantity = 1 },
            };

            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartId(cartId)).Returns(items);
            _postRepoMock.Setup(repo => repo.GetById(10)).Returns(new Post { Id = 10, Price = 100 });
            _postRepoMock.Setup(repo => repo.GetById(20)).Returns(new Post { Id = 20, Price = 50 });

            var result = _cartService.GetCartTotal(cartId);

            Assert.True(result.IsSuccess);
            Assert.Equal(250, result.Value);
        }

        [Fact]
        public void GetCartTotal_WhenProductNotFound_ShouldReturnFailure()
        {
            int cartId = 1;
            var items = new List<BuyerCart> { new BuyerCart { PostId = 999, Quantity = 1 } };

            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartId(cartId)).Returns(items);
            _postRepoMock.Setup(repo => repo.GetById(999)).Returns((Post)null!);

            var result = _cartService.GetCartTotal(cartId);

            Assert.True(result.IsFailure);
            Assert.Contains("товар з ID 999 не знайдено", result.ErrorMessage);
        }

        [Fact]
        public void ClearCart_WhenCartHasItems_ShouldCallRemoveRange()
        {
            int cartId = 1;
            var items = new List<BuyerCart> { new BuyerCart { CartId = 1 } };
            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartId(cartId)).Returns(items);

            var result = _cartService.ClearCart(cartId);

            Assert.True(result.IsSuccess);
            _buyerCartRepoMock.Verify(repo => repo.RemoveRange(items), Times.Once);
        }

        [Fact]
        public void ClearCart_WhenCartAlreadyEmpty_ShouldNotCallRemoveRange()
        {
            int cartId = 1;
            _buyerCartRepoMock.Setup(repo => repo.GetItemsByCartId(cartId)).Returns(new List<BuyerCart>());

            var result = _cartService.ClearCart(cartId);

            Assert.True(result.IsSuccess);
            _buyerCartRepoMock.Verify(repo => repo.RemoveRange(It.IsAny<IEnumerable<BuyerCart>>()), Times.Never);
        }

        [Fact]
        public void UpdateCartItem_WhenDataIsValid_ShouldCallUpdate()
        {
            var item = new BuyerCart { CartId = 1, PostId = 1, Quantity = 5 };

            var result = _cartService.UpdateCartItem(item);

            Assert.True(result.IsSuccess);
            _buyerCartRepoMock.Verify(repo => repo.UpdateItem(item), Times.Once);
        }

        [Fact]
        public void UpdateCartItem_WhenItemIsNull_ShouldReturnFailure()
        {
            var result = _cartService.UpdateCartItem(null!);

            Assert.True(result.IsFailure);
            _buyerCartRepoMock.Verify(repo => repo.UpdateItem(It.IsAny<BuyerCart>()), Times.Never);
        }
    }
}