namespace Inventory.Domain.Repositories
{
    public interface IUnitOfWork
    {
        IInventoryItemRepository InventoryItems { get; }
        IWarehouseRepository Warehouses { get; }
        IInventoryTransactionRepository Transactions { get; }
        IStockReservationRepository Reservations { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
