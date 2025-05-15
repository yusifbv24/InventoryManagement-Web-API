using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orders.Application.Interfaces;

namespace Ordes.Infrastructure.ExternalServices
{
    public class InventoryApiClient : IInventoryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public InventoryApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<InventoryApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configure base address from configuration
            _httpClient.BaseAddress = new Uri(configuration["Services:Inventory:Url"] ?? "http://inventory-api/api/");

            // Configure JSON serialization for case-insensitive property names
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<StockReservationResponse> ReserveStockAsync(ReserveStockRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Requesting stock reservation for order {OrderId} with {ItemCount} items",
                    request.OrderId, request.Items.Count);

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("v2/InventoryItems/reserve", content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<StockReservationResponse>(responseContent, _jsonOptions)
                        ?? new StockReservationResponse { Success = false };
                }

                _logger.LogWarning("Failed to reserve stock for order {OrderId}. Status code: {StatusCode}",
                    request.OrderId, response.StatusCode);

                return new StockReservationResponse { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Inventory API for stock reservation for order {OrderId}", request.OrderId);
                return new StockReservationResponse { Success = false };
            }
        }

        public async Task CommitReservationAsync(int orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Committing stock reservation for order {OrderId}", orderId);

                var response = await _httpClient.PostAsync($"v1/inventory/commit-reservation/{orderId}", null, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to commit reservation for order {OrderId}. Status code: {StatusCode}",
                        orderId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Inventory API to commit reservation for order {OrderId}", orderId);
            }
        }

        public async Task ReleaseReservationAsync(int orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Releasing stock reservation for order {OrderId}", orderId);

                var response = await _httpClient.PostAsync($"v1/inventory/release-reservation/{orderId}", null, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to release reservation for order {OrderId}. Status code: {StatusCode}",
                        orderId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Inventory API to release reservation for order {OrderId}", orderId);
            }
        }
    }
}