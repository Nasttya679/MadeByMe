using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task GetBuyerHistoryAsync_WhenOrdersExist_ShouldReturnOrderList()
        {
            string buyerId = "buyer-1";
            var orders = new List<Order> { new Order { Id = 1 }, new Order { Id = 2 } };
            _orderRepoMock.Setup(r => r.GetOrdersByBuyerIdAsync(buyerId)).ReturnsAsync(orders);

            var result = await _orderService.GetBuyerHistoryAsync(buyerId);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task GetBuyerHistoryAsync_WhenNoOrders_ShouldReturnEmptyList()
        {
            string buyerId = "buyer-1";
            _orderRepoMock.Setup(r => r.GetOrdersByBuyerIdAsync(buyerId)).ReturnsAsync(new List<Order>());

            var result = await _orderService.GetBuyerHistoryAsync(buyerId);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetSellerJournalAsync_WhenOrdersExist_ShouldReturnOrderList()
        {
            string sellerId = "seller-1";
            var orders = new List<Order> { new Order { Id = 1 } };
            _orderRepoMock.Setup(r => r.GetOrdersBySellerIdAsync(sellerId)).ReturnsAsync(orders);

            var result = await _orderService.GetSellerJournalAsync(sellerId);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
        }

        [Fact]
        public async Task GetSellerJournalAsync_WhenNoOrders_ShouldReturnEmptyList()
        {
            string sellerId = "seller-1";
            _orderRepoMock.Setup(r => r.GetOrdersBySellerIdAsync(sellerId)).ReturnsAsync(new List<Order>());

            var result = await _orderService.GetSellerJournalAsync(sellerId);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetSellerOrdersAsync_ShouldMapToDtoAndCalculateCorrectTotal()
        {
            string sellerId = "seller-1";
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { Quantity = 2, PriceAtPurchase = 50, Post = new Post { SellerId = sellerId, Title = "Item 1" } },
                        new OrderItem { Quantity = 1, PriceAtPurchase = 100, Post = new Post { SellerId = "other-seller" } },
                    },
                },
            };

            _orderRepoMock.Setup(r => r.GetOrdersBySellerIdAsync(sellerId)).ReturnsAsync(orders);

            var result = await _orderService.GetSellerOrdersAsync(sellerId);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);

            var dto = result.Value.First();
            Assert.Equal("John Doe", dto.BuyerName);
            Assert.Equal(100, dto.TotalPrice);
            Assert.Single(dto.Items);
            Assert.Equal("Item 1", dto.Items.First().ProductName);
        }

        [Fact]
        public async Task GetSellerOrdersAsync_WhenNoOrders_ShouldReturnEmptyDtoList()
        {
            string sellerId = "seller-1";
            _orderRepoMock.Setup(r => r.GetOrdersBySellerIdAsync(sellerId)).ReturnsAsync(new List<Order>());

            var result = await _orderService.GetSellerOrdersAsync(sellerId);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetSellerOrdersAsync_WithStatusFilter_ShouldReturnOnlyMatchingOrders()
        {
            string sellerId = "seller-1";
            var orders = new List<Order>
            {
                new Order { Id = 1, Status = "Processing", OrderItems = new List<OrderItem> { new OrderItem { Post = new Post { SellerId = sellerId } } } },
                new Order { Id = 2, Status = "Shipped", OrderItems = new List<OrderItem> { new OrderItem { Post = new Post { SellerId = sellerId } } } },
                new Order { Id = 3, Status = "Processing", OrderItems = new List<OrderItem> { new OrderItem { Post = new Post { SellerId = sellerId } } } },
            };
            _orderRepoMock.Setup(r => r.GetOrdersBySellerIdAsync(sellerId)).ReturnsAsync(orders);

            var result = await _orderService.GetSellerOrdersAsync(sellerId, "Processing");

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            Assert.All(result.Value, o => Assert.Equal("Processing", o.Status));
        }

        [Fact]
        public async Task GetSellerOrdersAsync_WithStatusAll_ShouldReturnAllOrdersIgnoringFilter()
        {
            string sellerId = "seller-1";
            var orders = new List<Order>
            {
                new Order { Id = 1, Status = "Processing", OrderItems = new List<OrderItem> { new OrderItem { Post = new Post { SellerId = sellerId } } } },
                new Order { Id = 2, Status = "Shipped", OrderItems = new List<OrderItem> { new OrderItem { Post = new Post { SellerId = sellerId } } } },
            };
            _orderRepoMock.Setup(r => r.GetOrdersBySellerIdAsync(sellerId)).ReturnsAsync(orders);

            var result = await _orderService.GetSellerOrdersAsync(sellerId, "All");

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetOrderByIdAndBuyerAsync_WhenValid_ShouldReturnOrder()
        {
            var expectedOrder = new Order { Id = 1, BuyerId = "buyer-1" };
            _orderRepoMock.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(expectedOrder);

            var result = await _orderService.GetOrderByIdAndBuyerAsync(1, "buyer-1");

            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("buyer-1", result.Value.BuyerId);
        }

        [Fact]
        public async Task GetOrderByIdAndBuyerAsync_WhenWrongBuyer_ShouldReturnFailure()
        {
            var existingOrder = new Order { Id = 1, BuyerId = "buyer-1" };
            _orderRepoMock.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(existingOrder);

            var result = await _orderService.GetOrderByIdAndBuyerAsync(1, "hacker-user");

            Assert.True(result.IsFailure);
            Assert.Equal("Замовлення не знайдено або у вас немає доступу.", result.ErrorMessage);
        }

        [Fact]
        public async Task CancelOrderAsync_WhenProcessing_ShouldCancelSuccessfully()
        {
            var order = new Order { Id = 1, BuyerId = "buyer-1", Status = "Processing" };
            _orderRepoMock.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(order);

            var result = await _orderService.CancelOrderAsync(1, "buyer-1", "Передумав");

            Assert.True(result.IsSuccess);
            Assert.Equal("Cancelled", order.Status);
            Assert.Equal("Передумав", order.CancellationReason);
            _orderRepoMock.Verify(r => r.UpdateOrderStatusAsync(1, "Cancelled"), Times.Once); // Перевіряємо чи збереглось в БД
        }

        [Fact]
        public async Task CancelOrderAsync_WhenAlreadyShipped_ShouldReturnFailure()
        {
            var order = new Order { Id = 1, BuyerId = "buyer-1", Status = "Shipped" };
            _orderRepoMock.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(order);

            var result = await _orderService.CancelOrderAsync(1, "buyer-1", "reason");

            Assert.True(result.IsFailure);
            Assert.Equal("Скасувати можна лише замовлення, які ще знаходяться в обробці.", result.ErrorMessage);
            _orderRepoMock.Verify(r => r.UpdateOrderStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never); // Перевіряємо що БД НЕ оновилась
        }
    }
}