namespace Inventory.Application.Messages
{
    public record InventoryLevelChangedMessage
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public int NewQuantity { get; set; }
        public int OldQuantity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
