using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs;
using Orders.Application.Services;
using Orders.Domain.Entities;

namespace Orders.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
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
        public async Task<ActionResult<OrdersListResponse>> GetOrders(
            [FromQuery] string? status = null,
            [FromQuery] string? customerEmail = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting orders with filters: Status={Status}, Customer={Customer}, FromDate={FromDate}, ToDate={ToDate}",
                status, customerEmail, fromDate, toDate);

            // Get all orders first
            var allOrders = await _orderService.GetAllOrdersAsync(cancellationToken);

            // Apply filters
            var filteredOrders = allOrders;

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                filteredOrders = filteredOrders.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(customerEmail))
            {
                filteredOrders = filteredOrders.Where(o => o.CustomerEmail.Equals(customerEmail, StringComparison.OrdinalIgnoreCase));
            }

            if (fromDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.OrderDate <= toDate.Value);
            }

            // Create summary statistics
            var totalOrders = filteredOrders.Count();
            var totalValue = filteredOrders.Sum(o => o.TotalAmount);
            var ordersByStatus = filteredOrders
                .GroupBy(o => o.Status)
                .Select(g => new OrdersListResponse.StatusSummary
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            var response = new OrdersListResponse
            {
                Orders = filteredOrders.ToList(),
                TotalCount = totalOrders,
                TotalValue = totalValue,
                SummaryStatus = ordersByStatus
            };

            return Ok(response);
        }

        [HttpGet("{id}/with-history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderWithHistoryDto>> GetOrderWithHistory(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting order with history: {OrderId}", id);

            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            if (order == null)
                return NotFound();

            var history = await _orderService.GetOrderStatusHistoryAsync(id, cancellationToken);

            var response = new OrderWithHistoryDto
            {
                Order = order,
                StatusHistory = history.ToList()
            };

            return Ok(response);
        }
    }

    public record OrdersListResponse
    {
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
        public int TotalCount { get; set; }
        public decimal TotalValue { get; set; }
        public List<StatusSummary> SummaryStatus { get; set; } = new List<StatusSummary>();

        public record StatusSummary
        {
            public string Status { get; set; } = string.Empty;
            public int Count { get; set; }
            public decimal TotalValue { get; set; }
        }
    }
    public record OrderWithHistoryDto
    {
        public OrderDto Order { get; set; } = null!;
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new List<OrderStatusHistoryDto>();
    }
}