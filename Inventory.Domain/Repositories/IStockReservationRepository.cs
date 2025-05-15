using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories
{
    public interface IStockReservationRepository
    {
        Task<IEnumerable<StockReservation>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<StockReservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockReservation>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockReservation>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockReservation>> GetActiveReservationsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);
        Task<StockReservation> AddAsync(StockReservation reservation, CancellationToken cancellationToken = default);
        Task UpdateAsync(StockReservation reservation, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }

}
