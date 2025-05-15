namespace Products.Application.Messages
{
    public record ProductDeletedMessage
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
