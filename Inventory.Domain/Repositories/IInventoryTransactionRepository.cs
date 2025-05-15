using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories
{
    public interface IInventoryTransactionRepository
    {
        Task<IEnumerable<InventoryTransaction>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<InventoryTransaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryTransaction>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryTransaction>> GetByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<InventoryTransaction> AddAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default);
    }
}
