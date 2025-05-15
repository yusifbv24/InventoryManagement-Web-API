using System.Text.Json;
using InventoryManagement.WebApp.Models.Inventory;

namespace InventoryManagement.WebApp.Services
{
    public class InventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public InventoryService(HttpClient httpClient, ILogger<InventoryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Inventory items API calls
        public async Task<ServiceResponse<List<InventoryItemViewModel>>> GetAllInventoryItemsAsync()
        {
            try
            {
                var items = await _httpClient.GetFromJsonAsync<List<InventoryItemViewModel>>("v1/inventoryitems", _jsonOptions);
                return new ServiceResponse<List<InventoryItemViewModel>>
                {
                    Success = true,
                    Data = items ?? new List<InventoryItemViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory items");
                return new ServiceResponse<List<InventoryItemViewModel>>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve inventory items. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<List<InventoryItemViewModel>>> GetLowStockItemsAsync()
        {
            try
            {
                var items = await _httpClient.GetFromJsonAsync<List<InventoryItemViewModel>>("v1/inventoryitems/low-stock", _jsonOptions);
                return new ServiceResponse<List<InventoryItemViewModel>>
                {
                    Success = true,
                    Data = items ?? new List<InventoryItemViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching low stock items");
                return new ServiceResponse<List<InventoryItemViewModel>>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve low stock items. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<InventoryItemViewModel>> AdjustInventoryAsync(AdjustInventoryViewModel adjustment)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("v1/inventoryitems/adjust", adjustment);
                if (response.IsSuccessStatusCode)
                {
                    var updatedItem = await response.Content.ReadFromJsonAsync<InventoryItemViewModel>(_jsonOptions);
                    return new ServiceResponse<InventoryItemViewModel>
                    {
                        Success = true,
                        Data = updatedItem
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ServiceResponse<InventoryItemViewModel>
                {
                    Success = false,
                    ErrorMessage = $"Failed to adjust inventory: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting inventory");
                return new ServiceResponse<InventoryItemViewModel>
                {
                    Success = false,
                    ErrorMessage = "Failed to adjust inventory. Please try again later."
                };
            }
        }

        // Warehouse API calls
        public async Task<ServiceResponse<List<WarehouseViewModel>>> GetAllWarehousesAsync()
        {
            try
            {
                var warehouses = await _httpClient.GetFromJsonAsync<List<WarehouseViewModel>>("v1/warehouses", _jsonOptions);
                return new ServiceResponse<List<WarehouseViewModel>>
                {
                    Success = true,
                    Data = warehouses ?? new List<WarehouseViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching warehouses");
                return new ServiceResponse<List<WarehouseViewModel>>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve warehouses. Please try again later."
                };
            }
        }
    }
}
