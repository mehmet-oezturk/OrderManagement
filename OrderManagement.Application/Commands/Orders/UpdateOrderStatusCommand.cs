using MediatR;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Entities;

namespace OrderManagement.Application.Commands.Orders;

public class UpdateOrderStatusCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
} 