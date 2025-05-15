using Inventory.Application.DTOs.InventoryItem;
using Inventory.Application.DTOs.StockReservation;
using Inventory.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.SignalR
{
    public class InventoryNotificationService : IInventoryNotificationService
    {
        private readonly IHubContext<InventoryHub> _hubContext;
        private readonly ILogger<InventoryNotificationService> _logger;

        public InventoryNotificationService(IHubContext<InventoryHub> hubContext, ILogger<InventoryNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyInventoryChanged(InventoryItemDto inventoryItem)
        {
            await _hubContext.Clients.Group("inventory").SendAsync("InventoryChanged", inventoryItem);
            _logger.LogInformation("Inventory changed notification sent: Product {ProductId}, Warehouse {WarehouseId}, Quantity {Quantity}",
                inventoryItem.ProductId, inventoryItem.WarehouseId, inventoryItem.Quantity);
        }

        public async Task NotifyLowStock(InventoryItemDto inventoryItem)
        {
            await _hubContext.Clients.Group("inventory").SendAsync("LowStock", inventoryItem);
            _logger.LogInformation("Low stock notification sent: Product {ProductId}, Warehouse {WarehouseId}, Quantity {Quantity}",
                inventoryItem.ProductId, inventoryItem.WarehouseId, inventoryItem.Quantity);
        }

        public async Task NotifyInventoryTransferred(InventoryItemDto sourceItem, InventoryItemDto destinationItem)
        {
            await _hubContext.Clients.Group("inventory").SendAsync("InventoryTransferred", new { Source = sourceItem, Destination = destinationItem });
            _logger.LogInformation("Inventory transfer notification sent: Product {ProductId}, From {SourceWarehouse} to {DestWarehouse}, Quantity {Quantity}",
                sourceItem.ProductId, sourceItem.WarehouseId, destinationItem.WarehouseId, sourceItem.Quantity - destinationItem.Quantity);
        }

        public async Task NotifyStockReserved(IEnumerable<StockReservationDto> reservations)
        {
            await _hubContext.Clients.Group("inventory").SendAsync("StockReserved", reservations);
            _logger.LogInformation("Stock reserved notification sent: Order {OrderId}, Items {Count}",
                reservations.FirstOrDefault()?.OrderId ?? 0, reservations.Count());
        }
    }
}
