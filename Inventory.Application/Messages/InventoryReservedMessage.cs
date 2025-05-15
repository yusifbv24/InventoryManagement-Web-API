namespace Inventory.Application.Messages
{
    public record InventoryReservedMessage
    {
        public int OrderId { get; set; }
        public List<ReservedItem> Items { get; set; } = new List<ReservedItem>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public record ReservedItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string ProductSku { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; } = string.Empty;
        }
    }
}
