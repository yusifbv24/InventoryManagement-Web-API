namespace Orders.Application.Messages
{
    public record InventoryInsufficientMessage
    {
        public int OrderId { get; set; }
        public List<InsufficientItem> Items { get; set; } = new List<InsufficientItem>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public record InsufficientItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string ProductSku { get; set; } = string.Empty;
            public int RequestedQuantity { get; set; }
            public int AvailableQuantity { get; set; }
        }
    }
}
