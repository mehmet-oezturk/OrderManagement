using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Queries.Orders;

public class GetOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public GetOrdersQuery(int page = 1, int pageSize = 20)
    {
        Page = page;
        PageSize = pageSize;
    }
}