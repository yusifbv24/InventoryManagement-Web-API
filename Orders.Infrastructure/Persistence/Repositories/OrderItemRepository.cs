using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;

namespace Ordes.Infrastructure.Persistence.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly OrdersDbContext _context;

        public OrderItemRepository(OrdersDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems
                .Where(i => i.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<OrderItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<OrderItem> AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            await _context.OrderItems.AddAsync(orderItem, cancellationToken);
            return orderItem;
        }

        public async Task UpdateAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            _context.Entry(orderItem).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var orderItem = await _context.OrderItems.FindAsync(new object[] { id }, cancellationToken);
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
            }
        }
    }
}