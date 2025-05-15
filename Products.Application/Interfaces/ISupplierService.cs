using Products.Application.DTOs;

namespace Products.Application.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default);
        Task<SupplierDto?> GetSupplierByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto, CancellationToken cancellationToken = default);
        Task UpdateSupplierAsync(UpdateSupplierDto updateSupplierDto, CancellationToken cancellationToken = default);
        Task DeleteSupplierAsync(int id, CancellationToken cancellationToken = default);
        Task ActivateSupplierAsync(int id, CancellationToken cancellationToken = default);
        Task DeactivateSupplierAsync(int id, CancellationToken cancellationToken = default);
    }
}
