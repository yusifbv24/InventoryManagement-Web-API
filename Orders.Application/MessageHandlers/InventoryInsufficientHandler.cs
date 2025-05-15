using Microsoft.Extensions.Logging;
using Orders.Application.Messages;
using Orders.Application.Services;
using Orders.Domain.Entities;

namespace Orders.Application.MessageHandlers
{
    public class InventoryInsufficientHandler
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<InventoryInsufficientHandler> _logger;

        public InventoryInsufficientHandler(
            IOrderService orderService,
            ILogger<InventoryInsufficientHandler> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task Handle(InventoryInsufficientMessage message)
        {
            _logger.LogInformation("Handling InventoryInsufficient message for order {OrderId}", message.OrderId);

            try
            {
                // Create details about insufficient inventory
                var insufficientDetails = string.Join(", ", message.Items.Select(i =>
                    $"{i.ProductName} (requested: {i.RequestedQuantity}, available: {i.AvailableQuantity})"));

                // Update order status with information about insufficient inventory
                await _orderService.UpdateOrderStatusAsync(new DTOs.UpdateOrderStatusDto
                {
                    OrderId = message.OrderId,
                    NewStatus = OrderStatus.PendingInventory,
                    Notes = $"Insufficient inventory: {insufficientDetails}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing InventoryInsufficient message for order {OrderId}", message.OrderId);
            }
        }
    }
}
