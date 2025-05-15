using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories
{
    public interface IWarehouseRepository
    {
        Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Warehouse> AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
        Task UpdateAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
