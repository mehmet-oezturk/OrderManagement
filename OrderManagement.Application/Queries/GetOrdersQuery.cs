using MediatR;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Application.Queries
{
    public class GetOrdersQuery : IRequest<IEnumerable<OrderDto>>
    {
        public string? CustomerId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IEnumerable<OrderDto>>
    {
        private readonly IOrderService _orderService;

        public GetOrdersQueryHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IEnumerable<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                return (IEnumerable<OrderDto>)await _orderService.GetOrdersAsync(request.CustomerId,1,10);
            }

            return (IEnumerable<OrderDto>)await _orderService.GetOrdersAsync(null, 1, 10);
        }
    }
} 