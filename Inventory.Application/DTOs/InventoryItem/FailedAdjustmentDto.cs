namespace Inventory.Application.DTOs.InventoryItem
{
    public record FailedAdjustmentDto
    {
        public int InventoryItemId { get; set; }
        public int Quantity { get; set; }
        public bool IsAddition { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
