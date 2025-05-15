using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Products.Application.Interfaces;
using Products.Application.Messages;
using Products.Infrastructure.Messaging.Handlers;

namespace Products.Infrastructure.Messaging
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageBusSubscriber> _logger;

        public MessageBusSubscriber(
            IServiceProvider serviceProvider,
            ILogger<MessageBusSubscriber> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting MessageBusSubscriber");

            using var scope = _serviceProvider.CreateScope();
            var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            // Subscribe to ProductDetailsRequested messages
            messageBus.Subscribe<ProductDetailsRequestedMessage>("product_details_requests", async message =>
            {
                using var handlerScope = _serviceProvider.CreateScope();
                var handler = handlerScope.ServiceProvider.GetRequiredService<ProductDetailsRequestedHandler>();
                await handler.Handle(message);
            });

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
