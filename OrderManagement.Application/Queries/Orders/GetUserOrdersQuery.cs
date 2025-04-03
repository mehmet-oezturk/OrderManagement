using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Queries.Orders;

public class GetUserOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public string UserId { get; set; }
    public GetUserOrdersQuery(string userId)
    {
        UserId = userId;
    }

} 