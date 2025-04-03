using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Queries.Orders;

public class GetOrderByIdQuery : IRequest<OrderDto>
{
    public Guid id;

    public GetOrderByIdQuery(Guid id)
    {
        this.id = id;
    }
} 