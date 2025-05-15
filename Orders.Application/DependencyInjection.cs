using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Mappings;
using Orders.Application.MessageHandlers;
using Orders.Application.Services;

namespace Orders.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Register application services
            services.AddScoped<IOrderService, OrderService>();

            // Register message handlers
            services.AddScoped<InventoryReservedHandler>();
            services.AddScoped<InventoryInsufficientHandler>();

            return services;
        }
    }
}
