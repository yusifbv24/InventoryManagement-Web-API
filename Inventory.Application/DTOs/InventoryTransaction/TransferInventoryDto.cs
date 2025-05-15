namespace Inventory.Application.DTOs.InventoryTransaction
{
    public record TransferInventoryDto
    {
        public int ProductId { get; set; }
        public int SourceWarehouseId { get; set; }
        public string SourceLocationCode { get; set; } = string.Empty;
        public int DestinationWarehouseId { get; set; }
        public string DestinationLocationCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
