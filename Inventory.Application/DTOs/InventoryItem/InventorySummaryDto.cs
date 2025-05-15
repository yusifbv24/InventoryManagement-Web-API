namespace Inventory.Application.DTOs.InventoryItem
{
    public record InventorySummaryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int WarehouseCount { get; set; }
        public int LowStockWarehouses { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
