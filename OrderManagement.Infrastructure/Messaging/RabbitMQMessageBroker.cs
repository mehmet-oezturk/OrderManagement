using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OrderManagement.Core.Entities;
using OrderManagement.Core.Events;
using OrderManagement.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderManagement.Infrastructure.Messaging
{
    public class RabbitMQMessageBroker : IMessageBroker, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQMessageBroker(string hostName = "localhost")
        {
            var factory = new ConnectionFactory { HostName = hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public async Task PublishAsync<T>(string topic, T message) where T : IOrderEvent
        {
            _channel.ExchangeDeclare(topic, ExchangeType.Fanout);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: topic,
                routingKey: "",
                basicProperties: null,
                body: body);

            await Task.CompletedTask;
        }

        public Task SubscribeAsync<T>(string topic, Func<T, Task> handler) where T : IOrderEvent
        {
            _channel.ExchangeDeclare(topic, ExchangeType.Fanout);

            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queueName, topic, "");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<T>(json);

                await handler(message);
            };

            _channel.BasicConsume(queue: queueName,
                                autoAck: true,
                                consumer: consumer);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}