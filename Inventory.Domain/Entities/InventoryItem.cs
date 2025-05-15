namespace Inventory.Domain.Entities
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderThreshold { get; set; }
        public int TargetStockLevel { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }

        // Business logic methods
        public bool IsLowStock => Quantity <= ReorderThreshold;

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            Quantity += quantity;
            LastUpdated = DateTime.UtcNow;
        }

        public void RemoveStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            if (quantity > Quantity)
                throw new InvalidOperationException($"Cannot remove {quantity} items. Only {Quantity} available.");

            Quantity -= quantity;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateStockLevels(int reorderThreshold, int targetStockLevel)
        {
            if (reorderThreshold < 0)
                throw new ArgumentException("Reorder threshold cannot be negative", nameof(reorderThreshold));

            if (targetStockLevel < reorderThreshold)
                throw new ArgumentException("Target stock level must be greater than or equal to reorder threshold", nameof(targetStockLevel));

            ReorderThreshold = reorderThreshold;
            TargetStockLevel = targetStockLevel;
            LastUpdated = DateTime.UtcNow;
        }
    }
}
