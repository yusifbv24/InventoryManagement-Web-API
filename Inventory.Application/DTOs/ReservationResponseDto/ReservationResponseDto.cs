namespace Inventory.Application.DTOs.ReservationResponseDto
{
    public record ReservationResponseDto
    {
        public bool Success { get; set; }
        public List<ReservedItemDto> ReservedItems { get; set; } = new List<ReservedItemDto>();
        public List<UnavailableItemDto> UnavailableItems { get; set; } = new List<UnavailableItemDto>();
    }

    public record ReservedItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
    }

    public record UnavailableItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
