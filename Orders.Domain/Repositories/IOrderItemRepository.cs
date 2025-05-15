using Orders.Domain.Entities;

namespace Orders.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<OrderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<OrderItem> AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
        Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
