using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;

namespace Ordes.Infrastructure.Persistence.Repositories
{
    public class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
    {
        private readonly OrdersDbContext _context;

        public OrderStatusHistoryRepository(OrdersDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.OrderStatusHistory
                .Where(h => h.OrderId == orderId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<OrderStatusHistory> AddAsync(OrderStatusHistory statusHistory, CancellationToken cancellationToken = default)
        {
            await _context.OrderStatusHistory.AddAsync(statusHistory, cancellationToken);
            return statusHistory;
        }
    }
}
