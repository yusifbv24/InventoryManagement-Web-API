namespace InventoryManagement.WebApp.Models.Orders
{
    public class UpdateOrderStatusViewModel
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
