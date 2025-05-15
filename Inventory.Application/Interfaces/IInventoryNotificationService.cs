using Inventory.Application.DTOs.InventoryItem;
using Inventory.Application.DTOs.StockReservation;

namespace Inventory.Application.Interfaces
{
    public interface IInventoryNotificationService
    {
        Task NotifyInventoryChanged(InventoryItemDto inventoryItem);
        Task NotifyLowStock(InventoryItemDto inventoryItem);
        Task NotifyInventoryTransferred(InventoryItemDto sourceItem, InventoryItemDto destinationItem);
        Task NotifyStockReserved(IEnumerable<StockReservationDto> reservations);
    }
}
