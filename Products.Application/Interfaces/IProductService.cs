using Products.Application.DTOs;

namespace Products.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
        Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductDto>> GetProductsBySupplierAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default);
        Task UpdateProductAsync(UpdateProductDto updateProductDto, CancellationToken cancellationToken = default);
        Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
        Task ActivateProductAsync(int id, CancellationToken cancellationToken = default);
        Task DeactivateProductAsync(int id, CancellationToken cancellationToken = default);
    }
}
