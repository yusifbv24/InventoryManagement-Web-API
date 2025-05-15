using Microsoft.Extensions.DependencyInjection;
using Products.Application.Interfaces;
using Products.Application.Mappings;
using Products.Application.Services;

namespace Products.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Register application services
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISupplierService, SupplierService>();

            return services;
        }
    }
}
