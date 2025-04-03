using OrderManagement.Core.Entities;

namespace OrderManagement.Core.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
}

public class CreateOrderDto
{
    public string UserId { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int Quantity { get; set; }
    public Guid Id { get; set; }
}