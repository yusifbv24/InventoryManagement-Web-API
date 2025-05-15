using Inventory.Application.Interfaces;
using Inventory.Domain.Repositories;
using Inventory.Infrastructure.ExternalServices;
using Inventory.Infrastructure.Persistence.Repositories;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Inventory.Infrastructure.Messaging;

namespace Inventory.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseNpgsql(
            configuration.GetConnectionString("InventoryDatabase"),
                    b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

            // Register repositories
            services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddScoped<IStockReservationRepository, StockReservationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register SignalR
            services.AddSignalR();
            services.AddScoped<IInventoryNotificationService, InventoryNotificationService>();

            // Register RabbitMQ message bus
            services.AddSingleton<IMessageBus>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQMessageBus>>();
                var hostName = configuration.GetSection("RabbitMQ:HostName").Value ?? "localhost";
                return new RabbitMQMessageBus(hostName, logger);
            });

            // Register Products API client
            services.AddHttpClient<IProductApiClient, ProductApiClient>();

            return services;
        }
    }
}
