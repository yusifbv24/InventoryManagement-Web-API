using System.Text.Json;
using InventoryManagement.WebApp.Models.Orders;

namespace InventoryManagement.WebApp.Services
{
    public class OrdersService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrdersService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public OrdersService(HttpClient httpClient, ILogger<OrdersService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ServiceResponse<List<OrderViewModel>>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _httpClient.GetFromJsonAsync<List<OrderViewModel>>("v1/orders", _jsonOptions);
                return new ServiceResponse<List<OrderViewModel>>
                {
                    Success = true,
                    Data = orders ?? new List<OrderViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return new ServiceResponse<List<OrderViewModel>>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve orders. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<OrderViewModel>> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _httpClient.GetFromJsonAsync<OrderViewModel>($"v1/orders/{id}", _jsonOptions);
                if (order == null)
                {
                    return new ServiceResponse<OrderViewModel>
                    {
                        Success = false,
                        ErrorMessage = $"Order with ID {id} not found."
                    };
                }

                return new ServiceResponse<OrderViewModel>
                {
                    Success = true,
                    Data = order
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId}", id);
                return new ServiceResponse<OrderViewModel>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve order. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<List<OrderStatusHistoryViewModel>>> GetOrderHistoryAsync(int id)
        {
            try
            {
                var history = await _httpClient.GetFromJsonAsync<List<OrderStatusHistoryViewModel>>($"v1/orders/{id}/history", _jsonOptions);
                return new ServiceResponse<List<OrderStatusHistoryViewModel>>
                {
                    Success = true,
                    Data = history ?? new List<OrderStatusHistoryViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order history for {OrderId}", id);
                return new ServiceResponse<List<OrderStatusHistoryViewModel>>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve order history. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<OrderViewModel>> CreateOrderAsync(CreateOrderViewModel order)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("v1/orders", order);
                if (response.IsSuccessStatusCode)
                {
                    var createdOrder = await response.Content.ReadFromJsonAsync<OrderViewModel>(_jsonOptions);
                    return new ServiceResponse<OrderViewModel>
                    {
                        Success = true,
                        Data = createdOrder
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ServiceResponse<OrderViewModel>
                {
                    Success = false,
                    ErrorMessage = $"Failed to create order: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return new ServiceResponse<OrderViewModel>
                {
                    Success = false,
                    ErrorMessage = "Failed to create order. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<OrderViewModel>> UpdateOrderStatusAsync(UpdateOrderStatusViewModel statusUpdate)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("v1/orders/status", statusUpdate);
                if (response.IsSuccessStatusCode)
                {
                    var updatedOrder = await response.Content.ReadFromJsonAsync<OrderViewModel>(_jsonOptions);
                    return new ServiceResponse<OrderViewModel>
                    {
                        Success = true,
                        Data = updatedOrder
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ServiceResponse<OrderViewModel>
                {
                    Success = false,
                    ErrorMessage = $"Failed to update order status: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return new ServiceResponse<OrderViewModel>
                {
                    Success = false,
                    ErrorMessage = "Failed to update order status. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<OrderViewModel>> CancelOrderAsync(int id, string? reason = null)
        {
            try
            {
                var request = new { Reason = reason };
                var response = await _httpClient.PostAsJsonAsync($"v1/orders/{id}/cancel", request);
                if (response.IsSuccessStatusCode)
                {
                    var cancelledOrder = await response.Content.ReadFromJsonAsync<OrderViewModel>(_jsonOptions);
                    return new ServiceResponse<OrderViewModel>
                    {
                        Success = true,
                        Data = cancelledOrder
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ServiceResponse<OrderViewModel>
                {
                    Success = false,
                    ErrorMessage = $"Failed to cancel order: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return new ServiceResponse<OrderViewModel>
                {
                    Success = false,
                    ErrorMessage = "Failed to cancel order. Please try again later."
                };
            }
        }
    }
}
