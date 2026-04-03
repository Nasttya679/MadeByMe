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

            _cartServiceMock.Setup(s => s.GetUserCartEntityAsync(buyerId)).ReturnsAsync(cart);

            var result = await _orderService.CreateOrderAsync(buyerId, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(100, result.Value.TotalAmount);
            _orderRepoMock.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_EmptyCart_ShouldReturnFailure()
        {
            _cartServiceMock.Setup(s => s.GetUserCartEntityAsync(It.IsAny<string>()))
                .ReturnsAsync("Ваш кошик порожній.");

            var result = await _orderService.CreateOrderAsync("u1", new OrderDto());

            Assert.True(result.IsFailure);
            Assert.Equal("Ваш кошик порожній.", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateOrder_ShouldClearCartAfterSuccess()
        {
            var cart = new Cart { CartId = 99, BuyerCarts = new List<BuyerCart> { new BuyerCart { Post = new Post() } } };
            _cartServiceMock.Setup(s => s.GetUserCartEntityAsync(It.IsAny<string>())).ReturnsAsync(cart);

            await _orderService.CreateOrderAsync("u1", new OrderDto());

            // Використовуємо асинхронний метод для Verify
            _cartServiceMock.Verify(s => s.ClearCartAsync(99), Times.Once);
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

        [Fact]
        public async Task GetUserOrderHistory_ValidUserId_ShouldReturnOrders()
        {
            var testUserId = "user-123";
            var mockOrders = new List<Order>
            {
                new Order { Id = 1, BuyerId = testUserId, TotalAmount = 500 },
                new Order { Id = 2, BuyerId = testUserId, TotalAmount = 1500 },
            };

            _orderRepoMock.Setup(repo => repo.GetOrdersByUserIdAsync(testUserId)).ReturnsAsync(mockOrders);

            // Act
            var result = await _orderService.GetUserOrderHistoryAsync(testUserId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal(1, result.Value.First().Id);
            _orderRepoMock.Verify(r => r.GetOrdersByUserIdAsync(testUserId), Times.Once);
        }

        [Fact]
        public async Task GetUserOrderHistory_EmptyUserId_ShouldReturnFailure()
        {
            var emptyUserId = string.Empty;

            var result = await _orderService.GetUserOrderHistoryAsync(emptyUserId);

            Assert.True(result.IsFailure);
            Assert.Equal("Ідентифікатор користувача не знайдено.", result.ErrorMessage);
        }

        [Fact]
        public async Task GetUserOrderHistory_WithItemsAndSellers_ShouldReturnCompleteData()
        {
            // Arrange
            var userId = "user-1";
            var mockOrders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Post = new Post { Title = "Vase", Seller = new ApplicationUser { UserName = "Master1" } },
                        },
                    },
                },
            };
            _orderRepoMock.Setup(r => r.GetOrdersByUserIdAsync(userId)).ReturnsAsync(mockOrders);

            // Act
            var result = await _orderService.GetUserOrderHistoryAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var orders = result.Value!;
            var order = orders.First();
            var item = order.OrderItems.First();
            var post = item.Post;
            Assert.NotNull(post);
            var seller = post!.Seller;
            Assert.NotNull(seller);

            Assert.Equal("Vase", post.Title);
            Assert.Equal("Master1", seller!.UserName);
        }

        [Fact]
        public async Task CreateOrder_MultipleItems_ShouldCalculateCorrectTotal()
        {
            // Arrange
            var cart = new Cart
            {
                BuyerCarts = new List<BuyerCart>
                {
                    new BuyerCart { Quantity = 2, Post = new Post { Price = 50 } },
                    new BuyerCart { Quantity = 1, Post = new Post { Price = 200 } },
                },
            };
            _cartServiceMock.Setup(s => s.GetUserCartEntityAsync(It.IsAny<string>())).ReturnsAsync(cart);

            // Act
            var result = await _orderService.CreateOrderAsync("u1", new OrderDto());

            // Assert
            Assert.Equal(300, result.Value.TotalAmount); // (2*50) + (1*200)
        }
    }
}