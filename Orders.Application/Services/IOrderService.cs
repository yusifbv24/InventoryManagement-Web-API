using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Application.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
        Task<OrderDto?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default);
        Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto updateOrderStatusDto, CancellationToken cancellationToken = default);
        Task<OrderDto> CancelOrderAsync(int id, string? reason = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(int orderId, CancellationToken cancellationToken = default);
    }
}
