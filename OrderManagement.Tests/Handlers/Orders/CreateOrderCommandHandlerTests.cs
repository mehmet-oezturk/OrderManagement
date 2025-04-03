using Moq;
using OrderManagement.Application.Commands.Orders;
using OrderManagement.Application.Handlers.Orders;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Events;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Tests.Handlers.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IMessageBroker> _messageBrokerMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _productServiceMock = new Mock<IProductService>();
        _messageBrokerMock = new Mock<IMessageBroker>();

        _handler = new CreateOrderCommandHandler(
            _orderServiceMock.Object,
            _productServiceMock.Object,
            _messageBrokerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidOrder_ReturnsOrderDto()
    {
        var userId = "test-user-id";
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 100,
            StockQuantity = 10,
        };

        var command = new CreateOrderCommand
        {
            UserId = userId,
            Items = new List<OrderItemCommand>
        {
            new() { ProductId = productId, Quantity = 2 }
        }
        };

        _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        _productServiceMock.Setup(x => x.UpdateStockAsync(productId, 8))
            .ReturnsAsync(product);  

        _orderServiceMock.Setup(x => x.CreateOrderAsync(It.IsAny<OrderDto>()))
            .ReturnsAsync((OrderDto order) => order);

        _messageBrokerMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<IOrderEvent>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(200, result.TotalAmount);
        Assert.Single(result.OrderItems);
        Assert.Equal(productId, result.OrderItems[0].ProductId);
        Assert.Equal(2, result.OrderItems[0].Quantity);
        Assert.Equal(100, result.OrderItems[0].UnitPrice);

        _productServiceMock.Verify(x => x.GetProductByIdAsync(productId), Times.Once);

        _productServiceMock.Verify(x => x.UpdateStockAsync(productId, 8), Times.Once);

        _orderServiceMock.Verify(x => x.CreateOrderAsync(It.IsAny<OrderDto>()), Times.Once);

        _messageBrokerMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<IOrderEvent>()), Times.Once);
    }



    [Fact]
    public async Task Handle_WithNonExistentProduct_ThrowsException()
    {
        var userId = "test-user-id";
        var productId = Guid.NewGuid();

        var command = new CreateOrderCommand
        {
            UserId = userId,
            Items = new List<OrderItemCommand>
            {
                new() { ProductId = productId, Quantity = 1 }
            }
        };

        _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync((ProductDto)null);

        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Product with ID {productId} not found", exception.Message);
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ThrowsException()
    {
        var userId = "test-user-id";
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 100,
            StockQuantity = 5
        };

        var command = new CreateOrderCommand
        {
            UserId = userId,
            Items = new List<OrderItemCommand>
            {
                new() { ProductId = productId, Quantity = 10 }
            }
        };

        _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Insufficient stock for product {product.Name}", exception.Message);
    }
} 