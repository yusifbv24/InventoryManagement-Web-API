using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orders.Application.Interfaces;

namespace Ordes.Infrastructure.ExternalServices
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<ProductApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configure base address from configuration
            _httpClient.BaseAddress = new Uri(configuration["Services:Products:Url"] ?? "http://products-api/api/");

            // Configure JSON serialization for case-insensitive property names
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Requesting product with ID {ProductId} from Products API", id);

                var response = await _httpClient.GetAsync($"v1/products/{id}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<ProductDto>(content, _jsonOptions);
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
            var result = new List<ProductDto>();

            if (!ids.Any())
                return result;

            try
            {
                // This is a bit of a workaround - ideally, the Products API would have a batch endpoint
                // This could be enhanced to use parallelism for better performance
                foreach (var id in ids.Distinct())
                {
                    var product = await GetProductByIdAsync(id, cancellationToken);
                    if (product != null)
                    {
                        result.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch-fetching products");
            }

            return result;
        }
    }
}