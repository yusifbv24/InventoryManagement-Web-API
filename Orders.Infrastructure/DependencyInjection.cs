using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Interfaces;
using Orders.Domain.Repositories;
using Ordes.Infrastructure.ExternalServices;
using Ordes.Infrastructure.Persistence.Repositories;
using Ordes.Infrastructure.Persistence;
using Ordes.Infrastructure.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordes.Infrastructure.Messaging;

namespace Ordes.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("OrdersDatabase"),
                    b => b.MigrationsAssembly(typeof(OrdersDbContext).Assembly.FullName)));

            // Register repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register SignalR
            services.AddSignalR();
            services.AddScoped<IOrderNotificationService, OrderNotificationService>();

            // Register RabbitMQ message bus
            services.AddSingleton<IMessageBus>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQMessageBus>>();
                var hostName = configuration.GetSection("RabbitMQ:HostName").Value ?? "localhost";
                return new RabbitMQMessageBus(hostName, logger);
            });

            // Register message subscriber as a hosted service
            services.AddHostedService<MessageBusSubscriber>();

            // Register HTTP clients for external APIs
            services.AddHttpClient<IProductApiClient, ProductApiClient>();
            services.AddHttpClient<IInventoryApiClient, InventoryApiClient>();

            return services;
        }
    }
}