namespace Inventory.Domain.Entities
{
    public class StockReservation
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Business logic
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

        public void Release()
        {
            IsActive = false;
        }
    }
}
