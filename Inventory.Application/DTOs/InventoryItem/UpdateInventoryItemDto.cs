namespace Inventory.Application.DTOs.InventoryItem
{
    public record UpdateInventoryItemDto
    {
        public int Id { get; set; }
        public int ReorderThreshold { get; set; }
        public int TargetStockLevel { get; set; }
    }
}
