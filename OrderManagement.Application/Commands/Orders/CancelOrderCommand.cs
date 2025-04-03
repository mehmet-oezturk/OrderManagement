using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Orders;

public class CancelOrderCommand : IRequest<OrderDto>
{
    public Guid id;

    public CancelOrderCommand(Guid id)
    {
        this.id = id;
    }
} 