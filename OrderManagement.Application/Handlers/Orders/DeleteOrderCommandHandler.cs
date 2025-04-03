using MediatR;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Commands.Orders;

namespace OrderManagement.Application.Handlers.Orders;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly IOrderService _orderService;

    public DeleteOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(request.id);
        if (order == null)
            return false;

        await _orderService.DeleteOrderAsync(order.Id);
        return true;
    }
} 