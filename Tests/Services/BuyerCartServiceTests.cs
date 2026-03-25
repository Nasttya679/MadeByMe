using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class BuyerCartServiceTests
    {
        private readonly Mock<IPostRepository> _postRepoMock;
        private readonly Mock<ICartRepository> _cartRepoMock;
        private readonly Mock<IBuyerCartRepository> _buyerCartRepoMock;
        private readonly BuyerCartService _buyerCartService;

        public BuyerCartServiceTests()
        {
            _postRepoMock = new Mock<IPostRepository>();
            _cartRepoMock = new Mock<ICartRepository>();
            _buyerCartRepoMock = new Mock<IBuyerCartRepository>();

            _buyerCartService = new BuyerCartService(
                _postRepoMock.Object,
                _cartRepoMock.Object,
                _buyerCartRepoMock.Object);
        }

        [Fact]
        public void AddToCart_WhenPostDoesNotExist_ShouldReturnFailure()
        {
            string userId = "buyer1";
            var dto = new AddToCartDto { PostId = 1, Quantity = 2 };

            _postRepoMock.Setup(repo => repo.GetById(dto.PostId)).Returns((Post)null!);

            var result = _buyerCartService.AddToCart(userId, dto);

            Assert.True(result.IsFailure);
            Assert.Equal("Товар не знайдено.", result.ErrorMessage);

            _cartRepoMock.Verify(repo => repo.AddCart(It.IsAny<Cart>()), Times.Never);
            _buyerCartRepoMock.Verify(repo => repo.AddItem(It.IsAny<BuyerCart>()), Times.Never);
        }

        [Fact]
        public void AddToCart_WhenCartIsNewAndItemIsNew_ShouldCreateCartAndAddNewItem()
        {
            string userId = "buyer1";
            var dto = new AddToCartDto { PostId = 1, Quantity = 2 };
            var post = new Post { Id = 1, Title = "Handmade Cup" };

            _postRepoMock.Setup(repo => repo.GetById(dto.PostId)).Returns(post);

            _cartRepoMock.Setup(repo => repo.GetCartByBuyerId(userId)).Returns((Cart)null!);

            var result = _buyerCartService.AddToCart(userId, dto);

            Assert.True(result.IsSuccess);

            _cartRepoMock.Verify(repo => repo.AddCart(It.Is<Cart>(c => c.BuyerId == userId)), Times.Once);

            _buyerCartRepoMock.Verify(repo => repo.AddItem(It.Is<BuyerCart>(i => i.PostId == dto.PostId && i.Quantity == dto.Quantity)), Times.Once);
        }

        [Fact]
        public void AddToCart_WhenItemAlreadyInCart_ShouldUpdateQuantity()
        {
            string userId = "buyer1";
            var dto = new AddToCartDto { PostId = 1, Quantity = 3 };
            var post = new Post { Id = 1 };
            var existingCart = new Cart { CartId = 100, BuyerId = userId };
            var existingItem = new BuyerCart { CartId = 100, PostId = 1, Quantity = 2 }; // Вже є 2 штуки

            _postRepoMock.Setup(repo => repo.GetById(dto.PostId)).Returns(post);
            _cartRepoMock.Setup(repo => repo.GetCartByBuyerId(userId)).Returns(existingCart);
            _buyerCartRepoMock.Setup(repo => repo.GetItem(existingCart.CartId, dto.PostId)).Returns(existingItem);

            var result = _buyerCartService.AddToCart(userId, dto);

            Assert.True(result.IsSuccess);

            Assert.Equal(5, existingItem.Quantity);

            _buyerCartRepoMock.Verify(repo => repo.UpdateItem(existingItem), Times.Once);
            _buyerCartRepoMock.Verify(repo => repo.AddItem(It.IsAny<BuyerCart>()), Times.Never);
        }

        [Fact]
        public void RemoveFromCart_WhenCartDoesNotExist_ShouldReturnFailure()
        {
            string userId = "buyer1";
            int postId = 1;

            _cartRepoMock.Setup(repo => repo.GetCartByBuyerId(userId)).Returns((Cart)null!);

            var result = _buyerCartService.RemoveFromCart(userId, postId);

            Assert.True(result.IsFailure);
            Assert.Equal("Кошик користувача не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public void RemoveFromCart_WhenItemNotInCart_ShouldReturnFailure()
        {
            string userId = "buyer1";
            int postId = 1;
            var existingCart = new Cart { CartId = 100, BuyerId = userId };

            _cartRepoMock.Setup(repo => repo.GetCartByBuyerId(userId)).Returns(existingCart);

            _buyerCartRepoMock.Setup(repo => repo.GetItem(existingCart.CartId, postId)).Returns((BuyerCart)null!);

            var result = _buyerCartService.RemoveFromCart(userId, postId);

            Assert.True(result.IsFailure);
            Assert.Equal("Товар не знайдено у кошику.", result.ErrorMessage);
            _buyerCartRepoMock.Verify(repo => repo.RemoveItem(It.IsAny<BuyerCart>()), Times.Never);
        }

        [Fact]
        public void RemoveFromCart_WhenItemExists_ShouldRemoveItemAndReturnSuccess()
        {
            string userId = "buyer1";
            int postId = 1;
            var existingCart = new Cart { CartId = 100, BuyerId = userId };
            var existingItem = new BuyerCart { CartId = 100, PostId = 1 };

            _cartRepoMock.Setup(repo => repo.GetCartByBuyerId(userId)).Returns(existingCart);
            _buyerCartRepoMock.Setup(repo => repo.GetItem(existingCart.CartId, postId)).Returns(existingItem);

            var result = _buyerCartService.RemoveFromCart(userId, postId);

            Assert.True(result.IsSuccess);

            _buyerCartRepoMock.Verify(repo => repo.RemoveItem(existingItem), Times.Once);
        }
    }
}