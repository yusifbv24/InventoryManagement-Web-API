using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories
{
    public class StockReservationRepository : IStockReservationRepository
    {
        private readonly InventoryDbContext _context;

        public StockReservationRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StockReservation>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<StockReservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<StockReservation>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .Where(r => r.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<StockReservation>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .Where(r => r.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<StockReservation>> GetActiveReservationsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .Where(r => r.IsActive && (!r.ExpiryDate.HasValue || r.ExpiryDate > DateTime.UtcNow))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .Where(r => r.IsActive && r.ExpiryDate.HasValue && r.ExpiryDate <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task<StockReservation> AddAsync(StockReservation reservation, CancellationToken cancellationToken = default)
        {
            await _context.Reservations.AddAsync(reservation, cancellationToken);
            return reservation;
        }

        public async Task UpdateAsync(StockReservation reservation, CancellationToken cancellationToken = default)
        {
            _context.Entry(reservation).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var reservation = await _context.Reservations.FindAsync(new object[] { id }, cancellationToken);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }
        }
    }
}
