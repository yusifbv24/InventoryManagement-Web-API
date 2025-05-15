using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly InventoryDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        // Repositories
        private IInventoryItemRepository? _inventoryItemRepository;
        private IWarehouseRepository? _warehouseRepository;
        private IInventoryTransactionRepository? _transactionRepository;
        private IStockReservationRepository? _reservationRepository;

        public UnitOfWork(InventoryDbContext context)
        {
            _context = context;
        }

        public IInventoryItemRepository InventoryItems => _inventoryItemRepository ?? new InventoryItemRepository(_context);
        public IWarehouseRepository Warehouses => _warehouseRepository ?? new WarehouseRepository(_context);
        public IInventoryTransactionRepository Transactions => _transactionRepository ?? new InventoryTransactionRepository(_context);
        public IStockReservationRepository Reservations => _reservationRepository ?? new StockReservationRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Transaction has not been started.");
            }
            try
            {
                await _transaction?.CommitAsync(cancellationToken)!;
            }
            finally
            {
                await _transaction.DisposeAsync()!;
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Transaction has not been started.");
            }
            try
            {
                await _transaction?.RollbackAsync(cancellationToken)!;
            }
            finally
            {
                await _transaction.DisposeAsync()!;
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
                _transaction?.Dispose();
            }
            _disposed = true;
        }
    }
}
