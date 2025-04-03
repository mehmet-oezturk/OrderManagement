using MediatR;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Interfaces;
using OrderManagement.Application.Queries.Orders;
using Microsoft.AspNetCore.Identity;
using OrderManagement.Core.Entities;

namespace OrderManagement.Application.Handlers.Orders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;

    public GetOrdersQueryHandler(IOrderService orderService, IUserService userService)
    {
        _orderService = orderService;
        _userService = userService;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _userService.GetUserId();
        var orders = await _orderService.GetOrdersAsync(userId.ToString(), request.Page,request.PageSize);
        return orders;
    }
} 