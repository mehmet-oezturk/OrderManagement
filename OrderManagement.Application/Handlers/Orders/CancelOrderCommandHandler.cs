using MediatR;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Commands.Orders;
using OrderManagement.Core.Entities;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Handlers.Orders;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IOrderService _orderService;

    public CancelOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(request.id);
        if (order == null)
            return null;

        order.Status = OrderStatus.Cancelled;
        var updatedOrder = await _orderService.UpdateOrderAsync(order);
        return updatedOrder;
    }
} 