using System;
using OrderManagement.Core.Entities;

namespace OrderManagement.Core.Events
{
    public interface IOrderEvent
    {
        Guid OrderId { get; }
        OrderStatus Status { get; }
        DateTime Timestamp { get; }
    }

    public class OrderStatusChangedEvent : IOrderEvent
    {
        public Guid OrderId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime Timestamp { get; private set; }
        public OrderStatus PreviousStatus { get; private set; }

        public OrderStatusChangedEvent(Guid orderId, OrderStatus status, OrderStatus previousStatus)
        {
            OrderId = orderId;
            Status = status;
            PreviousStatus = previousStatus;
            Timestamp = DateTime.UtcNow;
        }
    }
    public class OrderDeletedEvent : IOrderEvent
    {
        public Guid OrderId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime Timestamp { get; private set; }

        public OrderDeletedEvent(Guid orderId)
        {
            OrderId = orderId;
            Status = OrderStatus.Deleted;
            Timestamp = DateTime.UtcNow;
        }
    }
    public class OrderCreatedEvent : IOrderEvent
    {
        public Guid OrderId { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime Timestamp { get; set; }
        public OrderCreatedEvent(Guid orderId)
        {
            OrderId= orderId;
            Status = OrderStatus.Pending;
            Timestamp = DateTime.UtcNow;
        }
    }
} 