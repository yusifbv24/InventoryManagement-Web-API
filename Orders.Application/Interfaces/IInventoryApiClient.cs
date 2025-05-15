namespace Orders.Application.Interfaces
{
    public record ReserveStockRequest
    {
        public int OrderId { get; set; }
        public List<ReserveStockItemRequest> Items { get; set; } = new List<ReserveStockItemRequest>();
    }
    public record ReserveStockItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public record StockReservationResponse
    {
        public bool Success { get; set; }
        public List<ReservedItem> ReservedItems { get; set; } = new List<ReservedItem>();
        public List<UnavailableItem> UnavailableItems { get; set; } = new List<UnavailableItem>();
        public record ReservedItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; } = string.Empty;
        }
        public record UnavailableItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int RequestedQuantity { get; set; }
            public int AvailableQuantity { get; set; }
        }
    }
    public interface IInventoryApiClient
    {
        Task<StockReservationResponse> ReserveStockAsync(ReserveStockRequest request, CancellationToken cancellationToken = default);
        Task CommitReservationAsync(int orderId, CancellationToken cancellationToken = default);
        Task ReleaseReservationAsync(int orderId, CancellationToken cancellationToken = default);
    }
}