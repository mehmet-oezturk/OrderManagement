using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderManagement.Core.Events;
using OrderManagement.Core.Interfaces;
using OrderManagement.Infrastructure.Events;

namespace OrderManagement.Infrastructure.Messaging
{
    public class OrderStatusSubscriber
    {
        private readonly IMessageBroker _messageBroker;
        private readonly OrderStatusChangedEventHandler _eventHandler;

        public OrderStatusSubscriber(IMessageBroker messageBroker, OrderStatusChangedEventHandler eventHandler)
        {
            _messageBroker = messageBroker;
            _eventHandler = eventHandler;
        }

        public async Task SubscribeToOrderStatusChanged()
        {
            await _messageBroker.SubscribeAsync<OrderStatusChangedEvent>("order.statusChanged", async (orderEvent) =>
            {
                await _eventHandler.HandleAsync(orderEvent);
            });
        }
    }

}
