namespace InventoryManagement.WebApp.Models.Orders
{
    public class CreateOrderViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreateOrderItemViewModel> Items { get; set; } = new List<CreateOrderItemViewModel>();
    }
}
