using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly InventoryDbContext _context;

        public WarehouseRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Warehouses.ToListAsync(cancellationToken);
        }

        public async Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Warehouses.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Warehouse> AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
        {
            await _context.Warehouses.AddAsync(warehouse, cancellationToken);
            return warehouse;
        }

        public async Task UpdateAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
        {
            _context.Entry(warehouse).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var warehouse = await _context.Warehouses.FindAsync(new object[] { id }, cancellationToken);
            if (warehouse != null)
            {
                _context.Warehouses.Remove(warehouse);
            }
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Warehouses.AnyAsync(w => w.Id == id, cancellationToken);
        }
    }
}
