using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Application.Commands
{
    public class CreateOrderCommand : IRequest<Order>
    {
        public string UserId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order>
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = new Order
            {
                UserId = request.UserId,
                OrderItems = request.OrderItems,
                ShippingAddress = request.ShippingAddress,
                PaymentMethod = request.PaymentMethod,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            return await _orderRepository.CreateAsync(order);
        }
    }
} 