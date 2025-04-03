using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Orders;

public class UpdateOrderCommand : IRequest<OrderDto>
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
} 