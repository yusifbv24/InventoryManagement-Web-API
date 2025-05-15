namespace Inventory.Application.DTOs.InventoryItem
{
    public record AdjustInventoryDto
    {
        public int InventoryItemId { get; set; }
        public int Quantity { get; set; }
        public bool IsAddition { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
