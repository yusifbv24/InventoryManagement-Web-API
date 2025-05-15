using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Application.Interfaces;
using Orders.Application.MessageHandlers;
using Orders.Application.Messages;

namespace Ordes.Infrastructure.Messaging
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MessageBusSubscriber> _logger;

        public MessageBusSubscriber(
            IServiceProvider serviceProvider,
            IMessageBus messageBus,
            ILogger<MessageBusSubscriber> logger)
        {
            _serviceProvider = serviceProvider;
            _messageBus = messageBus;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting MessageBusSubscriber");

            // Subscribe to InventoryReserved events
            _messageBus.Subscribe<InventoryReservedMessage>("inventory_reserved_queue", async message =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<InventoryReservedHandler>();
                await handler.Handle(message);
            });

            // Subscribe to InventoryInsufficient events
            _messageBus.Subscribe<InventoryInsufficientMessage>("inventory_insufficient_queue", async message =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<InventoryInsufficientHandler>();
                await handler.Handle(message);
            });

            return Task.CompletedTask;
        }
    }

}
