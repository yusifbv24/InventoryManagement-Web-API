using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories
{
    public interface IInventoryItemRepository
    {
        Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<InventoryItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryItem>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryItem>> GetByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default);
        Task<InventoryItem?> GetByProductAndWarehouseAsync(int productId, int warehouseId, string locationCode, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
        Task<InventoryItem> AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
        Task UpdateAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
