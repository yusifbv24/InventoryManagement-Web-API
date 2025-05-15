using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Interfaces;
using Products.Domain.Repositories;
using Products.Infrastructure.Messaging.Handlers;
using Products.Infrastructure.Persistence.Repositories;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.SignalR;
using Microsoft.EntityFrameworkCore;
using Products.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace Products.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<ProductsDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("ProductsDatabase"),
                    b => b.MigrationsAssembly(typeof(ProductsDbContext).Assembly.FullName)));

            // Register repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register SignalR
            services.AddSignalR();
            services.AddScoped<IProductNotificationService, ProductNotificationService>();

            // Register RabbitMQ message bus
            services.AddSingleton<IMessageBus>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RabbitMQMessageBus>>();
                var hostName = configuration.GetSection("RabbitMQ:HostName").Value??"localhost";
                return new RabbitMQMessageBus(hostName, logger);
            });

            // Register message handlers
            services.AddScoped<ProductDetailsRequestedHandler>();

            // Setup message subscriptions
            services.AddHostedService<MessageBusSubscriber>();

            return services;
        }
    }
}
