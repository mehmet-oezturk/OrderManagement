using MediatR;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Queries.Orders;

namespace OrderManagement.Application.Handlers.Orders;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderService _orderService;

    public GetUserOrdersQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetOrdersAsync(request.UserId,1,20);
        return orders;
    }
} 