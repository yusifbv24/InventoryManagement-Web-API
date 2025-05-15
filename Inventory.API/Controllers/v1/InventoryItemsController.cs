using Asp.Versioning;
using Inventory.Application.DTOs.InventoryItem;
using Inventory.Application.DTOs.InventoryTransaction;
using Inventory.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAllInventoryItems(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all inventory items");
            var inventoryItems = await _inventoryService.GetAllInventoryItemsAsync(cancellationToken);
            return Ok(inventoryItems);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryItemDto>> GetInventoryItemById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting inventory item by ID: {InventoryItemId}", id);
            var inventoryItem = await _inventoryService.GetInventoryItemByIdAsync(id, cancellationToken);

            if (inventoryItem == null)
                return NotFound();

            return Ok(inventoryItem);
        }


        [HttpGet("product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetInventoryItemsByProductId(int productId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting inventory items by product ID: {ProductId}", productId);
            var inventoryItems = await _inventoryService.GetInventoryItemsByProductIdAsync(productId, cancellationToken);
            return Ok(inventoryItems);
        }


        [HttpGet("warehouse/{warehouseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetInventoryItemsByWarehouseId(int warehouseId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting inventory items by warehouse ID: {WarehouseId}", warehouseId);
            var inventoryItems = await _inventoryService.GetInventoryItemsByWarehouseIdAsync(warehouseId, cancellationToken);
            return Ok(inventoryItems);
        }


        [HttpGet("low-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetLowStockItems(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting low stock items");
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync(cancellationToken);
            return Ok(lowStockItems);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryItemDto>> CreateInventoryItem([FromBody] CreateInventoryItemDto createInventoryItemDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating inventory item for product {ProductId} in warehouse {WarehouseId}",
                createInventoryItemDto.ProductId, createInventoryItemDto.WarehouseId);

            try
            {
                var inventoryItem = await _inventoryService.CreateInventoryItemAsync(createInventoryItemDto, cancellationToken);
                return CreatedAtAction(nameof(GetInventoryItemById), new { id = inventoryItem.Id, version = "1.0" }, inventoryItem);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error creating inventory item");
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInventoryItem(int id, [FromBody] UpdateInventoryItemDto updateInventoryItemDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating inventory item: {InventoryItemId}", id);

            if (id != updateInventoryItemDto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in request body" });

            try
            {
                await _inventoryService.UpdateInventoryItemAsync(updateInventoryItemDto, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Inventory item not found for update: {InventoryItemId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error updating inventory item: {InventoryItemId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInventoryItem(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting inventory item: {InventoryItemId}", id);

            try
            {
                await _inventoryService.DeleteInventoryItemAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Inventory item not found for deletion: {InventoryItemId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error deleting inventory item: {InventoryItemId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("adjust")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryItemDto>> AdjustInventory([FromBody] AdjustInventoryDto adjustInventoryDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adjusting inventory for item {InventoryItemId}, {Operation} {Quantity}",
                adjustInventoryDto.InventoryItemId, adjustInventoryDto.IsAddition ? "adding" : "removing", adjustInventoryDto.Quantity);

            try
            {
                var inventoryItem = await _inventoryService.AdjustInventoryAsync(adjustInventoryDto, cancellationToken);
                return Ok(inventoryItem);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error adjusting inventory");
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("transfer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> TransferInventory([FromBody] TransferInventoryDto transferInventoryDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Transferring inventory of product {ProductId} from warehouse {SourceWarehouseId} to {DestinationWarehouseId}, quantity {Quantity}",
                transferInventoryDto.ProductId, transferInventoryDto.SourceWarehouseId, transferInventoryDto.DestinationWarehouseId, transferInventoryDto.Quantity);

            try
            {
                var (sourceItem, destinationItem) = await _inventoryService.TransferInventoryAsync(transferInventoryDto, cancellationToken);
                return Ok(new { Source = sourceItem, Destination = destinationItem });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error transferring inventory");
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
