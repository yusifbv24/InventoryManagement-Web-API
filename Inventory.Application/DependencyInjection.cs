using Inventory.Application.Mappings;
using Inventory.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Register application services
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();

            return services;
        }
    }
}
