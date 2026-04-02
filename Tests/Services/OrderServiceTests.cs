using MadeByMe.Application.DTOs;
using MadeByMe.Application.Services.Implementations;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Domain.Entities;
using MadeByMe.Infrastructure.Repositories.Interfaces;
using Moq;
using Serilog;
using Xunit;

namespace MadeByMe.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            Log.Logger = Serilog.Core.Logger.None;
            _orderRepoMock = new Mock<IOrderRepository>();
            _cartServiceMock = new Mock<ICartService>();
            _orderService = new OrderService(_orderRepoMock.Object, _cartServiceMock.Object);
        }

        [Fact]
        public async Task CreateOrder_ValidCart_ShouldReturnSuccess()
        {
            var buyerId = "user-1";
            var dto = new OrderDto { FirstName = "Veronika", CardNumber = "1234" };
            var cart = new Cart
            {
                CartId = 10,
                BuyerCarts = new List<BuyerCart>
                {
                    new BuyerCart { PostId = 1, Quantity = 1, Post = new Post { Price = 100 } },
                },
            };

            _cartServiceMock.Setup(s => s.GetUserCartEntity(buyerId)).Returns(MadeByMe.Application.Common.Result<Cart>.Success(cart));

            var result = await _orderService.CreateOrderAsync(buyerId, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(100, result.Value.TotalAmount);
            _orderRepoMock.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_EmptyCart_ShouldReturnFailure()
        {
            _cartServiceMock.Setup(s => s.GetUserCartEntity(It.IsAny<string>()))
                .Returns(MadeByMe.Application.Common.Result<Cart>.Failure("Empty"));

            var result = await _orderService.CreateOrderAsync("u1", new OrderDto());

            Assert.True(result.IsFailure);
            Assert.Equal("Ваш кошик порожній.", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateOrder_ShouldClearCartAfterSuccess()
        {
            var cart = new Cart { CartId = 99, BuyerCarts = new List<BuyerCart> { new BuyerCart { Post = new Post() } } };
            _cartServiceMock.Setup(s => s.GetUserCartEntity(It.IsAny<string>())).Returns(MadeByMe.Application.Common.Result<Cart>.Success(cart));

            await _orderService.CreateOrderAsync("u1", new OrderDto());

            _cartServiceMock.Verify(s => s.ClearCart(99), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_WhenOrderNotExists_ShouldReturnFailure()
        {
            _orderRepoMock.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync((Order)null!);

            var result = await _orderService.UpdateOrderStatusAsync(1, "Shipped", "admin");

            Assert.True(result.IsFailure);
            Assert.Equal("Замовлення не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateStatus_WhenUserIsNotSeller_ShouldReturnFailure()
        {
            var order = new Order { Id = 1, OrderItems = new List<OrderItem> { new OrderItem { Post = new Post { SellerId = "real-seller" } } } };
            _orderRepoMock.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(order);

            var result = await _orderService.UpdateOrderStatusAsync(1, "Shipped", "fake-seller");

            Assert.True(result.IsFailure);
            Assert.Contains("немає прав", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateStatus_ValidSeller_ShouldCallRepository()
        {
            var order = new Order { Id = 1, OrderItems = new List<OrderItem> { new OrderItem { Post = new Post { SellerId = "seller-1" } } } };
            _orderRepoMock.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(order);

            var result = await _orderService.UpdateOrderStatusAsync(1, "Delivered", "seller-1");

            Assert.True(result.IsSuccess);
            _orderRepoMock.Verify(r => r.UpdateOrderStatusAsync(1, "Delivered"), Times.Once);
        }
    }
}