using Microsoft.EntityFrameworkCore.Storage;
using Orders.Domain.Repositories;

namespace Ordes.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly OrdersDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        // Repositories
        private IOrderRepository? _orderRepository;
        private IOrderItemRepository? _orderItemRepository;
        private IOrderStatusHistoryRepository? _orderStatusHistoryRepository;

        public UnitOfWork(OrdersDbContext context)
        {
            _context = context;
        }

        public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);
        public IOrderItemRepository OrderItems => _orderItemRepository ??= new OrderItemRepository(_context);
        public IOrderStatusHistoryRepository OrderStatusHistory => _orderStatusHistoryRepository ??= new OrderStatusHistoryRepository(_context);

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
            if(_transaction == null)
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
