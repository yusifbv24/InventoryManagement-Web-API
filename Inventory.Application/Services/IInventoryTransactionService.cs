using Inventory.Application.DTOs.InventoryTransaction;

namespace Inventory.Application.Services
{
    public interface IInventoryTransactionService
    {
        Task<IEnumerable<InventoryTransactionDto>> GetAllTransactionsAsync(CancellationToken cancellationToken = default);
        Task<InventoryTransactionDto?> GetTransactionByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
