using Azure;
using OrderManagement.Core.DTOs;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Events;
using OrderManagement.Core.Interfaces;
using OrderManagement.Infrastructure.Repositories;
using StackExchange.Redis;
using static OrderManagement.Infrastructure.Messaging.RabbitMQMessageBroker;

namespace OrderManagement.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMessageBroker _messageBroker;
    private readonly RedisCacheService _redisCacheService;

    public OrderService(IOrderRepository orderRepository, IMessageBroker messageBroker,RedisCacheService redisCacheService)
    {
        _orderRepository = orderRepository;
        _messageBroker = messageBroker;
        _redisCacheService = redisCacheService;
    }

    public async Task DeleteOrderAsync(Guid id)
    {
        await _orderRepository.DeleteAsync(id);
        var deletedOrderEvent = new OrderDeletedEvent(id);
        await _messageBroker.PublishAsync("order.deleted", deletedOrderEvent);
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid id)
    {
        string cacheKey = $"order:{id}";

        return await _redisCacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return MapToOrderDto(order);
        });
    }

    public async Task<PagedResult<OrderDto>> GetOrdersAsync(string? userId, int page, int pageSize)
    {
        string cacheKey = $"orders:user:{userId}:page:{page}:size:{pageSize}";

        return await _redisCacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var orders = await _orderRepository.GetUserOrdersAsync(userId != null ? userId : null, page, pageSize);
            var totalOrders = (await _orderRepository.GetByUserIdAsync(userId != null ? userId : null)).Count();

            return new PagedResult<OrderDto>
            {
                Items = MapToOrderDtos(orders),
                TotalCount = totalOrders
            };
        });
    }

    public async Task<OrderDto> CreateOrderAsync(OrderDto order)
    {
        var createOrder = new Core.Entities.Order
        {
            UserId = order.UserId,
            CreatedAt = DateTime.UtcNow,
            PaymentMethod = order.PaymentMethod,
            ShippingAddress = order.ShippingAddress,
            Status = OrderStatus.Pending,
            TotalAmount = order.TotalAmount,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = (List<OrderItem>)MapToOrderItems(order.OrderItems)
        };

        var createdOrder = await _orderRepository.AddAsync(createOrder);

        var orderCreatedEvent = new OrderCreatedEvent(createdOrder.Id);
        await _messageBroker.PublishAsync("order.created", orderCreatedEvent);

        var cacheKey = $"orders:user:{order.UserId}";
        var cachedOrders = await _redisCacheService.GetOrSetAsync<List<OrderDto>>(cacheKey, () => Task.FromResult(new List<OrderDto>()));

        if (cachedOrders != null)
        {
            cachedOrders.Add(MapToOrderDto(createdOrder));
            await _redisCacheService.GetOrSetAsync(cacheKey, () => Task.FromResult(cachedOrders));
        }
        else
        {
            await _redisCacheService.RemoveAsync(cacheKey);
        }

        return MapToOrderDto(createdOrder);
    }

    public async Task<OrderDto> UpdateOrderAsync(OrderDto order)
    {
        var updateOrder = await _orderRepository.GetByIdAsync(order.Id);
        if (updateOrder == null)
            return null;
        updateOrder.PaymentMethod = order.PaymentMethod;
        updateOrder.ShippingAddress = order.ShippingAddress;
        updateOrder.TotalAmount = order.TotalAmount;
        updateOrder.UpdatedAt = DateTime.UtcNow;

        var updatedOrder = await _orderRepository.UpdateAsync(updateOrder);
       

        await _redisCacheService.RemoveAsync($"order:{order.Id}");
        await _redisCacheService.RemoveAsync($"orders:user:{order.UserId}");

        return MapToOrderDto(updatedOrder);
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var updateOrder = await _orderRepository.GetByIdAsync(orderId);

        if (updateOrder == null)
            return false;

        if (updateOrder.Status != status)
        {
            var orderStatusChangedEvent = new OrderStatusChangedEvent(
                updateOrder.Id,
                status,
                updateOrder.Status
            );
            await _messageBroker.PublishAsync("order.statusChanged", orderStatusChangedEvent);
        }
        
        return true;
    }

    private OrderDto MapToOrderDto(Core.Entities.Order order)
    {
        var orderDto = new OrderDto
        {
            Id = order.Id,
            PaymentMethod = order.PaymentMethod,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            UserId = order.UserId,
            OrderItems = MapToOrderItemDtos(order.OrderItems).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
        
        return orderDto;
        
    }

    private IEnumerable<OrderDto> MapToOrderDtos(IEnumerable<Core.Entities.Order> orders)
    {
        return orders.Select(MapToOrderDto);
    }
    private OrderItemDto MapToOrderItemDto(OrderItem orderItem)
    {
        var orderItemDto = new OrderItemDto
        {
            Id = orderItem.Id,
            OrderId = orderItem.OrderId,
            ProductId = orderItem.ProductId,
            ProductName = orderItem.ProductName,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice,
            TotalPrice = orderItem.UnitPrice * orderItem.Quantity
        };
        
        return orderItemDto;

    }

    private IEnumerable<OrderItemDto> MapToOrderItemDtos(IEnumerable<OrderItem> orderItems)
    {
        return orderItems.Select(MapToOrderItemDto);
    }

    private OrderItem MapToOrderItem(OrderItemDto item)
    {
        var orderItem = new OrderItem
        {
            Id = item.Id,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.UnitPrice * item.Quantity
        };

        return orderItem;
    }

    private IEnumerable<OrderItem> MapToOrderItems(IEnumerable<OrderItemDto> orderItems)
    {
        return orderItems.Select(MapToOrderItem);
    }
} 