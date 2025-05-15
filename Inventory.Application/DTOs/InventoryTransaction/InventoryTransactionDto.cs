namespace Inventory.Application.DTOs.InventoryTransaction
{
    public record InventoryTransactionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? SourceWarehouseId { get; set; }
        public string? SourceWarehouseName { get; set; }
        public int? DestinationWarehouseId { get; set; }
        public string? DestinationWarehouseName { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
