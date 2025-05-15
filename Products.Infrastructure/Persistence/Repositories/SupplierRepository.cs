using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Infrastructure.Persistence.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ProductsDbContext _context;

        public SupplierRepository(ProductsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.ToListAsync(cancellationToken);
        }

        public async Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            await _context.Suppliers.AddAsync(supplier, cancellationToken);
            return supplier;
        }

        public async Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default)
        {
            _context.Entry(supplier).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var supplier = await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
            }
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.Email == email, cancellationToken);
        }
    }
}
