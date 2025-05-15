using Orders.Domain.Entities;

namespace Orders.Application.DTOs
{
    public record UpdateOrderStatusDto
    {
        public int OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string? Notes { get; set; }
    }
}
