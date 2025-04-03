using OrderManagement.Core.DTOs;
using OrderManagement.Core.Entities;

namespace OrderManagement.Core.Interfaces;

public interface IOrderService
{
    Task<OrderDto> GetOrderByIdAsync(Guid id);
    Task<PagedResult<OrderDto>> GetOrdersAsync(string? userId, int page, int pageSize);
    Task<OrderDto> CreateOrderAsync(OrderDto order);
    Task<OrderDto> UpdateOrderAsync(OrderDto order);
    Task DeleteOrderAsync(Guid id);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
} 