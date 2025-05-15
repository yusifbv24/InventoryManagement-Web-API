namespace Inventory.Application.Messages
{
    public record LowStockAlertMessage
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int ReorderThreshold { get; set; }
        public int TargetStockLevel { get; set; }
        public int SuggestedOrderQuantity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
