using Orders.Domain.Entities;

namespace Orders.Domain.Repositories
{
    public interface IOrderStatusHistoryRepository
    {
        Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<OrderStatusHistory> AddAsync(OrderStatusHistory statusHistory, CancellationToken cancellationToken = default);
    }
}
