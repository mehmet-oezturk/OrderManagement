using MediatR;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Commands.Orders;
using OrderManagement.Core.Entities;

namespace OrderManagement.Application.Handlers.Orders;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;

    public UpdateOrderCommandHandler(IOrderService orderService, IProductService productService)
    {
        _orderService = orderService;
        _productService = productService;
    }

    public async Task<OrderDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(request.Id);
        if (order == null)
            return null;

        decimal totalAmount = 0;
        var orderItems = new List<OrderItemDto>();

        foreach (var item in request.OrderItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null || product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Product with ID {item.ProductId} is not available in the requested quantity.");

            var orderItem = new OrderItemDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += orderItem.UnitPrice * orderItem.Quantity;

            var stockQuantity = product.StockQuantity;
            stockQuantity -= item.Quantity;
            await _productService.UpdateStockAsync(item.ProductId, stockQuantity);
        }

        order.OrderItems = orderItems;
        order.TotalAmount = totalAmount;
        order.UpdatedAt = DateTime.UtcNow;
        return await _orderService.UpdateOrderAsync(order);
    }
} 