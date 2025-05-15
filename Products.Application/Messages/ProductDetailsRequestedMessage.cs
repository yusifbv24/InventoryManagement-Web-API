namespace Products.Application.Messages
{
    public record ProductDetailsRequestedMessage
    {
        public int OrderId { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public record OrderItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
