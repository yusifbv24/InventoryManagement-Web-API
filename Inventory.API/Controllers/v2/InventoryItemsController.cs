using Asp.Versioning;
using Inventory.Application.DTOs.InventoryItem;
using Inventory.Application.DTOs.ReservationResponseDto;
using Inventory.Application.DTOs.StockReservation;
using Inventory.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class InventoryItemsController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryItemsController> _logger;

        public InventoryItemsController(IInventoryService inventoryService, ILogger<InventoryItemsController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventorySummaryDto>>> GetInventorySummary(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting inventory summary");

            var inventoryItems = await _inventoryService.GetAllInventoryItemsAsync(cancellationToken);

            var summary = inventoryItems
                .GroupBy(i => new { i.ProductId, i.ProductName, i.ProductSku })
                .Select(g => new InventorySummaryDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    ProductSku = g.Key.ProductSku,
                    TotalQuantity = g.Sum(i => i.Quantity),
                    WarehouseCount = g.Select(i => i.WarehouseId).Distinct().Count(),
                    LowStockWarehouses = g.Count(i => i.IsLowStock),
                    LastUpdated = g.Max(i => i.LastUpdated)
                })
                .ToList();

            return Ok(summary);
        }

        [HttpPost("batch-adjust")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BatchAdjustmentResultDto>> BatchAdjustInventory(
            [FromBody] List<AdjustInventoryDto> adjustments,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Batch adjusting inventory for {Count} items", adjustments.Count);

            var result = new BatchAdjustmentResultDto
            {
                SuccessfulAdjustments = new List<InventoryItemDto>(),
                FailedAdjustments = new List<FailedAdjustmentDto>()
            };

            foreach (var adjustment in adjustments)
            {
                try
                {
                    var inventoryItem = await _inventoryService.AdjustInventoryAsync(adjustment, cancellationToken);
                    result.SuccessfulAdjustments.Add(inventoryItem);
                }
                catch (Exception ex)
                {
                    result.FailedAdjustments.Add(new FailedAdjustmentDto
                    {
                        InventoryItemId = adjustment.InventoryItemId,
                        Quantity = adjustment.Quantity,
                        IsAddition = adjustment.IsAddition,
                        Error = ex.Message
                    });
                }
            }

            return Ok(result);
        }


        [HttpPost("reserve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReservationResponseDto>> ReserveStock(
            [FromBody] ReserveStockDto reserveStockDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reserving stock for order {OrderId} with {ItemCount} items",
                reserveStockDto.OrderId, reserveStockDto.Items.Count);

            if (reserveStockDto.OrderId <= 0)
                return BadRequest(new { message = "Invalid order ID" });

            if (reserveStockDto.Items.Count == 0)
                return BadRequest(new { message = "No items to reserve" });

            try
            {
                var reservations = await _inventoryService.ReserveStockAsync(reserveStockDto, cancellationToken);

                // Check if all items were successfully reserved
                var allProductIds = reserveStockDto.Items.Select(i => i.ProductId).ToHashSet();
                var reservedProductIds = reservations.Select(r => r.ProductId).ToHashSet();
                var unavailableProductIds = allProductIds.Except(reservedProductIds).ToList();

                // If we have unavailable items, find out details for them
                var unavailableItems = new List<UnavailableItemDto>();
                if (unavailableProductIds.Any())
                {
                    foreach (var productId in unavailableProductIds)
                    {
                        var requestedItem = reserveStockDto.Items.First(i => i.ProductId == productId);
                        var inventoryItems = await _inventoryService.GetInventoryItemsByProductIdAsync(productId, cancellationToken);
                        var availableQuantity = inventoryItems.Sum(i => i.Quantity);

                        unavailableItems.Add(new UnavailableItemDto
                        {
                            ProductId = productId,
                            ProductName = inventoryItems.FirstOrDefault()?.ProductName ?? "Unknown",
                            ProductSku = inventoryItems.FirstOrDefault()?.ProductSku ?? "Unknown",
                            RequestedQuantity = requestedItem.Quantity,
                            AvailableQuantity = availableQuantity
                        });
                    }
                }

                var response = new ReservationResponseDto
                {
                    Success = unavailableItems.Count == 0,
                    ReservedItems = reservations.Select(r => new ReservedItemDto
                    {
                        ProductId = r.ProductId,
                        ProductName = r.ProductName,
                        ProductSku = r.ProductSku,
                        Quantity = r.Quantity,
                        WarehouseId = r.WarehouseId,
                        WarehouseName = r.WarehouseName
                    }).ToList(),
                    UnavailableItems = unavailableItems
                };

                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error reserving stock for order {OrderId}", reserveStockDto.OrderId);
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("commit-reservation/{orderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CommitReservation(
            int orderId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Committing stock reservation for order {OrderId}", orderId);

            if (orderId <= 0)
                return BadRequest(new { message = "Invalid order ID" });

            try
            {
                await _inventoryService.CommitReservationAsync(orderId, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Reservation not found for order {OrderId}", orderId);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error committing reservation for order {OrderId}", orderId);
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("release-reservation/{orderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReleaseReservation(
            int orderId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Releasing stock reservation for order {OrderId}", orderId);

            if (orderId <= 0)
                return BadRequest(new { message = "Invalid order ID" });

            try
            {
                await _inventoryService.ReleaseReservationAsync(orderId, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Reservation not found for order {OrderId}", orderId);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error releasing reservation for order {OrderId}", orderId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
