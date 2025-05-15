using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories
{
    public class InventoryItemRepository : IInventoryItemRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryItemRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Warehouse)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<InventoryItem>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Warehouse)
                .Where(i => i.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InventoryItem>> GetByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Warehouse)
                .Where(i => i.WarehouseId == warehouseId)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryItem?> GetByProductAndWarehouseAsync(int productId, int warehouseId, string locationCode, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i =>
                    i.ProductId == productId &&
                    i.WarehouseId == warehouseId &&
                    i.LocationCode == locationCode,
                    cancellationToken);
        }

        public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Warehouse)
                .Where(i => i.Quantity <= i.ReorderThreshold)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryItem> AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
        {
            await _context.InventoryItems.AddAsync(inventoryItem, cancellationToken);
            return inventoryItem;
        }

        public async Task UpdateAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
        {
            _context.Entry(inventoryItem).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(new object[] { id }, cancellationToken);
            if (inventoryItem != null)
            {
                _context.InventoryItems.Remove(inventoryItem);
            }
        }
    }
}
