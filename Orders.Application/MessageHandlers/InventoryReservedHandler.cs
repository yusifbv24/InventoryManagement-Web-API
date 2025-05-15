using Microsoft.Extensions.Logging;
using Orders.Application.Messages;
using Orders.Application.Services;
using Orders.Domain.Entities;

namespace Orders.Application.MessageHandlers
{
    public class InventoryReservedHandler
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<InventoryReservedHandler> _logger;

        public InventoryReservedHandler(
            IOrderService orderService,
            ILogger<InventoryReservedHandler> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task Handle(InventoryReservedMessage message)
        {
            _logger.LogInformation("Handling InventoryReserved message for order {OrderId}", message.OrderId);

            try
            {
                // Update order status to Reserved
                await _orderService.UpdateOrderStatusAsync(new DTOs.UpdateOrderStatusDto
                {
                    OrderId = message.OrderId,
                    NewStatus = OrderStatus.Reserved,
                    Notes = $"Inventory reserved for {message.Items.Count} products"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing InventoryReserved message for order {OrderId}", message.OrderId);
            }
        }
    }
}
