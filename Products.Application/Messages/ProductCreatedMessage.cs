namespace Products.Application.Messages
{
    public record ProductCreatedMessage
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
