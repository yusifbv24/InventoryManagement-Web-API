namespace InventoryManagement.WebApp.Models.Orders
{
    public class OrderStatusHistoryViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
    }
}
