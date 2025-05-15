namespace Inventory.Application.DTOs.StockReservation
{
    public record ReserveStockDto
    {
        public int OrderId { get; set; }
        public List<ReserveStockItemDto> Items { get; set; } = new List<ReserveStockItemDto>();
        public TimeSpan ReservationDuration { get; set; } = TimeSpan.FromHours(24);
    }

    public record ReserveStockItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
