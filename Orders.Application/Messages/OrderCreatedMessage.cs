namespace Orders.Application.Messages
{
    public record OrderCreatedMessage
    {
        public int OrderId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public List<OrderItemMessage> Items { get; set; } = new List<OrderItemMessage>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public record OrderItemMessage
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}