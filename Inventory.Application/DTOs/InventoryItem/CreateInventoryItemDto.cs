namespace Inventory.Application.DTOs.InventoryItem
{
    public record CreateInventoryItemDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderThreshold { get; set; }
        public int TargetStockLevel { get; set; }
    }
}
