namespace Orders.Application.Messages
{
    public record OrderCancelledMessage
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
