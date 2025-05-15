using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Orders.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ordes.Infrastructure.Messaging
{
    public class RabbitMQMessageBus : IMessageBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQMessageBus> _logger;
        private readonly Dictionary<string, IModel> _consumerChannels;
        private bool _disposed;

        public RabbitMQMessageBus(string hostName, ILogger<RabbitMQMessageBus> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory { HostName = hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumerChannels = new Dictionary<string, IModel>();
        }

        public void Publish<T>(T message, string exchangeName, string routingKey)
        {
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, true);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Message published to {Exchange} with routing key {RoutingKey}",
                exchangeName, routingKey);
        }

        public void Subscribe<T>(string queueName, Action<T> handler)
        {
            // Create a new channel for this consumer
            var consumerChannel = _connection.CreateModel();
            _consumerChannels[queueName] = consumerChannel;

            consumerChannel.QueueDeclare(queueName, true, false, false);

            var consumer = new EventingBasicConsumer(consumerChannel);
            consumer.Received += (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var deserializedMessage = JsonSerializer.Deserialize<T>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (deserializedMessage != null)
                    {
                        handler(deserializedMessage);
                        consumerChannel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                    consumerChannel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            consumerChannel.BasicConsume(queueName, false, consumer);

            _logger.LogInformation("Subscribed to queue {QueueName}", queueName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var channel in _consumerChannels.Values)
                {
                    channel.Close();
                    channel.Dispose();
                }

                _channel.Close();
                _channel.Dispose();
                _connection.Close();
                _connection.Dispose();
            }

            _disposed = true;
        }
    }
}
