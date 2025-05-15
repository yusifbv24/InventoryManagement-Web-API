namespace Orders.Domain.Entities
{
    public enum OrderStatus
    {
        Created,         // Initial state when order is created
        PendingInventory, // Waiting for inventory reservation
        Reserved,        // Inventory has been reserved
        Processing,      // Order is being processed
        Shipped,         // Order has been shipped
        Delivered,       // Order has been delivered
        Cancelled,       // Order has been cancelled
        Returned         // Order has been returned
    }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

        // Business logic methods
        public void AddItem(OrderItem item)
        {
            Items.Add(item);
            UpdateTotalAmount();
        }

        public void UpdateStatus(OrderStatus newStatus, string? notes = null)
        {
            if (Status == newStatus)
                return;

            var previousStatus = Status;
            Status = newStatus;

            // Record status change in history
            StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = Id,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow,
                Notes = notes
            });

            // Update relevant dates based on status
            switch (newStatus)
            {
                case OrderStatus.Shipped:
                    ShippedDate = DateTime.UtcNow;
                    break;

                case OrderStatus.Delivered:
                    DeliveredDate = DateTime.UtcNow;
                    break;
            }
        }

        private void UpdateTotalAmount()
        {
            TotalAmount = Items.Sum(item => item.Price * item.Quantity);
        }

        public bool CanCancel()
        {
            // Orders can be cancelled if they haven't been shipped yet
            return Status != OrderStatus.Shipped &&
                   Status != OrderStatus.Delivered &&
                   Status != OrderStatus.Cancelled &&
                   Status != OrderStatus.Returned;
        }

        public bool CanUpdateItems()
        {
            // Items can be updated only in early stages
            return Status == OrderStatus.Created ||
                   Status == OrderStatus.PendingInventory;
        }
    }
}
