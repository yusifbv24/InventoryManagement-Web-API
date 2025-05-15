using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Infrastructure.SignalR
{
    public class ProductNotificationService : IProductNotificationService
    {
        private readonly IHubContext<ProductHub> _hubContext;
        private readonly ILogger<ProductNotificationService> _logger;

        public ProductNotificationService(IHubContext<ProductHub> hubContext, ILogger<ProductNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyProductCreated(ProductDto product)
        {
            await _hubContext.Clients.Group("products").SendAsync("ProductCreated", product);
            _logger.LogInformation("Product created notification sent: {ProductId}", product.Id);
        }

        public async Task NotifyProductUpdated(ProductDto product)
        {
            await _hubContext.Clients.Group("products").SendAsync("ProductUpdated", product);
            _logger.LogInformation("Product updated notification sent: {ProductId}", product.Id);
        }

        public async Task NotifyProductDeleted(int productId)
        {
            await _hubContext.Clients.Group("products").SendAsync("ProductDeleted", productId);
            _logger.LogInformation("Product deleted notification sent: {ProductId}", productId);
        }

        public async Task NotifyCategoryCreated(CategoryDto category)
        {
            await _hubContext.Clients.Group("categories").SendAsync("CategoryCreated", category);
            _logger.LogInformation("Category created notification sent: {CategoryId}", category.Id);
        }

        public async Task NotifyCategoryUpdated(CategoryDto category)
        {
            await _hubContext.Clients.Group("categories").SendAsync("CategoryUpdated", category);
            _logger.LogInformation("Category updated notification sent: {CategoryId}", category.Id);
        }

        public async Task NotifyCategoryDeleted(int categoryId)
        {
            await _hubContext.Clients.Group("categories").SendAsync("CategoryDeleted", categoryId);
            _logger.LogInformation("Category deleted notification sent: {CategoryId}", categoryId);
        }

        public async Task NotifySupplierCreated(SupplierDto supplier)
        {
            await _hubContext.Clients.Group("suppliers").SendAsync("SupplierCreated", supplier);
            _logger.LogInformation("Supplier created notification sent: {SupplierId}", supplier.Id);
        }

        public async Task NotifySupplierUpdated(SupplierDto supplier)
        {
            await _hubContext.Clients.Group("suppliers").SendAsync("SupplierUpdated", supplier);
            _logger.LogInformation("Supplier updated notification sent: {SupplierId}", supplier.Id);
        }

        public async Task NotifySupplierDeleted(int supplierId)
        {
            await _hubContext.Clients.Group("suppliers").SendAsync("SupplierDeleted", supplierId);
            _logger.LogInformation("Supplier deleted notification sent: {SupplierId}", supplierId);
        }
    }
}
