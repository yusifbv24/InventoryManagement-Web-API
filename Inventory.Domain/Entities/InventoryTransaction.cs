namespace Inventory.Domain.Entities
{
    public enum TransactionType
    {
        Received,
        Shipped,
        Adjusted,
        Transferred,
        Reserved,
        Released,
        Returned
    }

    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? SourceWarehouseId { get; set; }
        public int? DestinationWarehouseId { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
