using OrderManagement.Core.Events;
using OrderManagement.Core.Interfaces;

namespace OrderManagement.Infrastructure.Events
{
    public class OrderStatusChangedEventHandler
    {
        private readonly IOrderService _orderService;

        public OrderStatusChangedEventHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task HandleAsync(OrderStatusChangedEvent orderEvent)
        {

            var order = await _orderService.GetOrderByIdAsync(orderEvent.OrderId);
            if (order == null)
            {
                Console.WriteLine($"❌ Sipariş bulunamadı: {orderEvent.OrderId}");
                return;
            }

            await _orderService.UpdateOrderStatusAsync(order.Id,orderEvent.Status);

        }
    }
} 