using MediatR;
using OrderManagement.Core.DTOs;

namespace OrderManagement.Application.Commands.Orders;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public string UserId { get; set; }
    public List<OrderItemCommand> Items { get; set; } = new();
    public string ShippingAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
}

public class OrderItemCommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
} 