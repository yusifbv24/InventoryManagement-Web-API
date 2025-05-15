using Inventory.Application.DTOs.InventoryItem;
using Inventory.Application.DTOs.InventoryTransaction;
using Inventory.Application.DTOs.StockReservation;

namespace Inventory.Application.Services
{
    public interface IInventoryService
    {
        // Inventory Items management
        Task<IEnumerable<InventoryItemDto>> GetAllInventoryItemsAsync(CancellationToken cancellationToken = default);
        Task<InventoryItemDto?> GetInventoryItemByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryItemDto>> GetInventoryItemsByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryItemDto>> GetInventoryItemsByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryItemDto>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
        Task<InventoryItemDto> CreateInventoryItemAsync(CreateInventoryItemDto createInventoryItemDto, CancellationToken cancellationToken = default);
        Task UpdateInventoryItemAsync(UpdateInventoryItemDto updateInventoryItemDto, CancellationToken cancellationToken = default);
        Task DeleteInventoryItemAsync(int id, CancellationToken cancellationToken = default);

        // Inventory adjustments
        Task<InventoryItemDto> AdjustInventoryAsync(AdjustInventoryDto adjustInventoryDto, CancellationToken cancellationToken = default);
        Task<(InventoryItemDto Source, InventoryItemDto Destination)> TransferInventoryAsync(TransferInventoryDto transferInventoryDto, CancellationToken cancellationToken = default);

        // Stock reservations
        Task<IEnumerable<StockReservationDto>> ReserveStockAsync(ReserveStockDto reserveStockDto, CancellationToken cancellationToken = default);
        Task CommitReservationAsync(int orderId, CancellationToken cancellationToken = default);
        Task ReleaseReservationAsync(int orderId, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockReservationDto>> GetReservationsByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    }
}
