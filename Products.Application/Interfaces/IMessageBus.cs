namespace Products.Application.Interfaces
{
    public interface IMessageBus
    {
        void Publish<T>(T message, string exchangeName, string routingKey);
        void Subscribe<T>(string queueName, Action<T> handler);
    }
}
