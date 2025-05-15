using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs;
using Orders.Application.Services;
using Orders.Domain.Entities;

namespace Orders.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all orders");
            var orders = await _orderService.GetAllOrdersAsync(cancellationToken);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting order by ID: {OrderId}", id);
            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpGet("customer/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomer(string email, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting orders for customer: {CustomerEmail}", email);
            var orders = await _orderService.GetOrdersByCustomerEmailAsync(email, cancellationToken);
            return Ok(orders);
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByStatus(string status, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting orders with status: {Status}", status);

            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                return BadRequest(new { message = $"Invalid order status: {status}" });

            var orders = await _orderService.GetOrdersByStatusAsync(orderStatus, cancellationToken);
            return Ok(orders);
        }

        [HttpGet("recent/{count:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetRecentOrders(int count, CancellationToken cancellationToken)
        {
            if (count <= 0)
                return BadRequest(new { message = "Count must be greater than zero" });

            _logger.LogInformation("Getting {Count} recent orders", count);
            var orders = await _orderService.GetRecentOrdersAsync(count, cancellationToken);
            return Ok(orders);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating order for customer: {CustomerName}, {CustomerEmail}",
                createOrderDto.CustomerName, createOrderDto.CustomerEmail);

            try
            {
                var order = await _orderService.CreateOrderAsync(createOrderDto, cancellationToken);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id, version = "1.0" }, order);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error creating order");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus([FromBody] UpdateOrderStatusDto updateOrderStatusDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating order {OrderId} status to {NewStatus}",
                updateOrderStatusDto.OrderId, updateOrderStatusDto.NewStatus);

            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(updateOrderStatusDto, cancellationToken);
                return Ok(order);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Order not found for status update: {OrderId}", updateOrderStatusDto.OrderId);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error updating order status");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> CancelOrder(int id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cancelling order: {OrderId}", id);

            try
            {
                var order = await _orderService.CancelOrderAsync(id, request.Reason, cancellationToken);
                return Ok(order);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Order not found for cancellation: {OrderId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error cancelling order");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderStatusHistoryDto>>> GetOrderStatusHistory(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting status history for order: {OrderId}", id);

            // Check if order exists
            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            if (order == null)
                return NotFound();

            var history = await _orderService.GetOrderStatusHistoryAsync(id, cancellationToken);
            return Ok(history);
        }
    }
    public record CancelOrderRequest
    {
        public string? Reason { get; set; }
    }
}
