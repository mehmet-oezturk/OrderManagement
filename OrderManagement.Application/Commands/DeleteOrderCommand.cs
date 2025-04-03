using MediatR;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Application.Commands
{
    public class DeleteOrderCommand : IRequest
    {
        public Guid OrderId { get; set; }
    }

    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
    {
        private readonly IOrderService _orderService;

        public DeleteOrderCommandHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            await _orderService.DeleteOrderAsync(request.OrderId);
        }
    }
} 