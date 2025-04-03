using MediatR;
using OrderManagement.Application.Commands.Orders;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Events;
using OrderManagement.Core.Interfaces;
using static OrderManagement.Infrastructure.Messaging.RabbitMQMessageBroker;

namespace OrderManagement.Application.Handlers.Orders;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IMessageBroker _messageBroker;

    public CreateOrderCommandHandler(
        IOrderService orderService,
        IProductService productService,
        IMessageBroker messageBroker)
    {
        _orderService = orderService;
        _productService = productService;
        _messageBroker = messageBroker;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new OrderDto
        {
            UserId = request.UserId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;
        foreach (var item in request.Items)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null)
                throw new Exception($"Product with ID {item.ProductId} not found");

            if (product.StockQuantity < item.Quantity)
                throw new Exception($"Insufficient stock for product {product.Name}");

            var orderItem = new OrderItemDto
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                TotalPrice = item.Quantity * product.Price
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.UnitPrice * orderItem.Quantity;
            var stockQuantity = product.StockQuantity;
            stockQuantity -=  item.Quantity;
            await _productService.UpdateStockAsync(orderItem.ProductId,stockQuantity);
        }

        order.TotalAmount = totalAmount;
        var createdOrder = await _orderService.CreateOrderAsync(order);

        await _messageBroker.PublishAsync("order.created", new OrderCreatedEvent(order.Id));


        return createdOrder;
    }
}