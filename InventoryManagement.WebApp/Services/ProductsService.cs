using System.Text.Json;
using InventoryManagement.WebApp.Models.Products;

namespace InventoryManagement.WebApp.Services
{
    public class ProductsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductsService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductsService(HttpClient httpClient, ILogger<ProductsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Products API calls
        public async Task<ServiceResponse<List<ProductViewModel>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<ProductViewModel>>("v1/products", _jsonOptions);
                return new ServiceResponse<List<ProductViewModel>>
                {
                    Success = true,
                    Data = products ?? new List<ProductViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return new ServiceResponse<List<ProductViewModel>>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve products. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<ProductViewModel>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductViewModel>($"v1/products/{id}", _jsonOptions);
                if (product == null)
                {
                    return new ServiceResponse<ProductViewModel>
                    {
                        Success = false,
                        ErrorMessage = $"Product with ID {id} not found."
                    };
                }

                return new ServiceResponse<ProductViewModel>
                {
                    Success = true,
                    Data = product
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId}", id);
                return new ServiceResponse<ProductViewModel>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve product. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<ProductViewModel>> CreateProductAsync(CreateProductViewModel product)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("v1/products", product);
                if (response.IsSuccessStatusCode)
                {
                    var createdProduct = await response.Content.ReadFromJsonAsync<ProductViewModel>(_jsonOptions);
                    return new ServiceResponse<ProductViewModel>
                    {
                        Success = true,
                        Data = createdProduct
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ServiceResponse<ProductViewModel>
                {
                    Success = false,
                    ErrorMessage = $"Failed to create product: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return new ServiceResponse<ProductViewModel>
                {
                    Success = false,
                    ErrorMessage = "Failed to create product. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateProductAsync(int id, UpdateProductViewModel product)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"v1/products/{id}", product);
                if (response.IsSuccessStatusCode)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = true,
                        Data = true
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ServiceResponse<bool>
                {
                    Success = false,
                    ErrorMessage = $"Failed to update product: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    ErrorMessage = "Failed to update product. Please try again later."
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"v1/products/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = true,
                        Data = true
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ServiceResponse<bool>
                {
                    Success = false,
                    ErrorMessage = $"Failed to delete product: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    ErrorMessage = "Failed to delete product. Please try again later."
                };
            }
        }

        // Category API calls
        public async Task<ServiceResponse<List<CategoryViewModel>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _httpClient.GetFromJsonAsync<List<CategoryViewModel>>("v1/categories", _jsonOptions);
                return new ServiceResponse<List<CategoryViewModel>>
                {
                    Success = true,
                    Data = categories ?? new List<CategoryViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return new ServiceResponse<List<CategoryViewModel>>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve categories. Please try again later."
                };
            }
        }

        // Supplier API calls
        public async Task<ServiceResponse<List<SupplierViewModel>>> GetAllSuppliersAsync()
        {
            try
            {
                var suppliers = await _httpClient.GetFromJsonAsync<List<SupplierViewModel>>("v1/suppliers", _jsonOptions);
                return new ServiceResponse<List<SupplierViewModel>>
                {
                    Success = true,
                    Data = suppliers ?? new List<SupplierViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suppliers");
                return new ServiceResponse<List<SupplierViewModel>>
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve suppliers. Please try again later."
                };
            }
        }
    }
}
