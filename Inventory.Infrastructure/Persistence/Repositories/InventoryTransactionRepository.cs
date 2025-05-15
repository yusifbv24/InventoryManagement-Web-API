using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryTransactionRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Warehouse)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryTransaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Warehouse)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Warehouse)
                .Where(t => t.ProductId == productId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Warehouse)
                .Where(t => t.WarehouseId == warehouseId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Include(t => t.Warehouse)
                .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryTransaction> AddAsync(InventoryTransaction transaction, CancellationToken cancellationToken = default)
        {
            await _context.Transactions.AddAsync(transaction, cancellationToken);
            return transaction;
        }
    }
}
