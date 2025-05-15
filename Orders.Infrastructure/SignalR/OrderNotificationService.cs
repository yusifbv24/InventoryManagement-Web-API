using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;

namespace Ordes.Infrastructure.SignalR
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILogger<OrderNotificationService> _logger;

        public OrderNotificationService(IHubContext<OrderHub> hubContext, ILogger<OrderNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyOrderCreated(OrderDto order)
        {
            await _hubContext.Clients.Group("orders").SendAsync("OrderCreated", order);
            await _hubContext.Clients.Group($"customer-{order.CustomerEmail}").SendAsync("OrderCreated", order);

            _logger.LogInformation("Order created notification sent: Order ID {OrderId}, Customer {CustomerEmail}",
                order.Id, order.CustomerEmail);
        }

        public async Task NotifyOrderStatusChanged(OrderDto order, string previousStatus)
        {
            await _hubContext.Clients.Group("orders").SendAsync("OrderStatusChanged", new { Order = order, PreviousStatus = previousStatus });
            await _hubContext.Clients.Group($"customer-{order.CustomerEmail}").SendAsync("OrderStatusChanged", new { Order = order, PreviousStatus = previousStatus });

            _logger.LogInformation("Order status changed notification sent: Order ID {OrderId}, Status {OldStatus} -> {NewStatus}",
                order.Id, previousStatus, order.Status);
        }

        public async Task NotifyOrderCancelled(OrderDto order)
        {
            await _hubContext.Clients.Group("orders").SendAsync("OrderCancelled", order);
            await _hubContext.Clients.Group($"customer-{order.CustomerEmail}").SendAsync("OrderCancelled", order);

            _logger.LogInformation("Order cancelled notification sent: Order ID {OrderId}", order.Id);
        }
    }
}
