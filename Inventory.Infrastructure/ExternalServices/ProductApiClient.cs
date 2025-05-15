using System.Text.Json;
using Inventory.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.ExternalServices
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;

        public ProductApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<ProductApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configure the base address from configuration
            _httpClient.BaseAddress = new Uri(configuration["Services:Products:Url"] ?? "http://products-api/api/");
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"v1/products/{id}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);

                    // Create options for case-insensitive property matching
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    return JsonSerializer.Deserialize<ProductDto>(content, options);
                }

                _logger.LogWarning("Failed to get product {ProductId}. Status code: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Products API for product {ProductId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            if (!ids.Any())
                return new List<ProductDto>();

            var result = new List<ProductDto>();

            //retrieve products one by one
            foreach (var id in ids)
            {
                var product = await GetProductByIdAsync(id, cancellationToken);
                if (product != null)
                {
                    result.Add(product);
                }
            }

            return result;
        }
    }
}
