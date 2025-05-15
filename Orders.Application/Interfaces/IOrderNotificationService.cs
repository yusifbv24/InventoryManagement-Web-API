using Orders.Application.DTOs;

namespace Orders.Application.Interfaces
{
    public interface IOrderNotificationService
    {
        Task NotifyOrderCreated(OrderDto order);
        Task NotifyOrderStatusChanged(OrderDto order, string previousStatus);
        Task NotifyOrderCancelled(OrderDto order);
    }
}
