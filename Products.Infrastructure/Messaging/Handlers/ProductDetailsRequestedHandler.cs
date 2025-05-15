using Microsoft.Extensions.Logging;
using Products.Application.Interfaces;
using Products.Application.Messages;
using Products.Domain.Repositories;

namespace Products.Infrastructure.Messaging.Handlers
{
    public class ProductDetailsRequestedHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<ProductDetailsRequestedHandler> _logger;

        public ProductDetailsRequestedHandler(
            IUnitOfWork unitOfWork,
            IMessageBus messageBus,
            ILogger<ProductDetailsRequestedHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _messageBus = messageBus;
            _logger = logger;
        }

        public async Task Handle(ProductDetailsRequestedMessage message)
        {
            _logger.LogInformation("Handling ProductDetailsRequested for OrderId: {OrderId}", message.OrderId);

            var productInfos = new List<ProductDetailInfo>();

            foreach (var item in message.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    productInfos.Add(new ProductDetailInfo
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        SKU = product.SKU,
                        Price = product.Price,
                        CategoryId = product.CategoryId,
                        CategoryName = product.Category?.Name ?? string.Empty,
                        Weight = product.Weight,
                        Dimensions = product.Dimensions,
                        ImageUrl = product.ImageUrl,
                        RequestedQuantity = item.Quantity
                    });
                }
                else
                {
                    _logger.LogWarning("Product not found for ID: {ProductId}", item.ProductId);
                }
            }

            var response = new ProductDetailsProvidedMessage
            {
                OrderId = message.OrderId,
                Products = productInfos,
                Timestamp = DateTime.UtcNow
            };

            _messageBus.Publish(response, "product_events", "product.details.provided");
            _logger.LogInformation("Published ProductDetailsProvided for OrderId: {OrderId}", message.OrderId);
        }
    }

    // Helper class for the response message
    public record ProductDetailsProvidedMessage
    {
        public int OrderId { get; set; }
        public List<ProductDetailInfo> Products { get; set; } = new List<ProductDetailInfo>();
        public DateTime Timestamp { get; set; }
    }

    public record ProductDetailInfo
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public string Dimensions { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
    }
}
