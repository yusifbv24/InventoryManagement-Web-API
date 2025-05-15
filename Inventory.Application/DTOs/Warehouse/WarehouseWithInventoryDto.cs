using Inventory.Application.DTOs.InventoryItem;

namespace Inventory.Application.DTOs.Warehouse
{
    public record WarehouseWithInventoryDto : WarehouseDto
    {
        public int TotalProducts { get; set; }
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public List<InventoryItemDto> InventoryItems { get; set; } = new List<InventoryItemDto>();
    }
}
