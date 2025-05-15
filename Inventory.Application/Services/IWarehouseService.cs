using Inventory.Application.DTOs.Warehouse;

namespace Inventory.Application.Services
{
    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync(CancellationToken cancellationToken = default);
        Task<WarehouseDto?> GetWarehouseByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto createWarehouseDto, CancellationToken cancellationToken = default);
        Task UpdateWarehouseAsync(UpdateWarehouseDto updateWarehouseDto, CancellationToken cancellationToken = default);
        Task DeleteWarehouseAsync(int id, CancellationToken cancellationToken = default);
        Task ActivateWarehouseAsync(int id, CancellationToken cancellationToken = default);
        Task DeactivateWarehouseAsync(int id, CancellationToken cancellationToken = default);
    }
}
