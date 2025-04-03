using System.Threading.Tasks;
using OrderManagement.Core.Events;

namespace OrderManagement.Core.Interfaces
{
    public interface IMessageBroker
    {
        Task PublishAsync<T>(string topic, T message) where T : IOrderEvent;
        Task SubscribeAsync<T>(string topic, Func<T, Task> handler) where T : IOrderEvent;
    }
} 