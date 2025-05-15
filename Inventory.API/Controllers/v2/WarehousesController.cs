using Asp.Versioning;
using Inventory.Application.DTOs.Warehouse;
using Inventory.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<WarehousesController> _logger;

        public WarehousesController(
            IWarehouseService warehouseService,
            IInventoryService inventoryService,
            ILogger<WarehousesController> logger)
        {
            _warehouseService = warehouseService;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses(
            [FromQuery] bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting warehouses with isActive filter: {IsActive}", isActive);

            var warehouses = await _warehouseService.GetAllWarehousesAsync(cancellationToken);

            if (isActive.HasValue)
                warehouses = warehouses.Where(w => w.IsActive == isActive.Value);

            return Ok(warehouses);
        }

        [HttpGet("{id}/with-inventory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WarehouseWithInventoryDto>> GetWarehouseWithInventory(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting warehouse with inventory: {WarehouseId}", id);

            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, cancellationToken);
            if (warehouse == null)
                return NotFound();

            var inventoryItems = await _inventoryService.GetInventoryItemsByWarehouseIdAsync(id, cancellationToken);

            var result = new WarehouseWithInventoryDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location,
                Address = warehouse.Address,
                ContactPerson = warehouse.ContactPerson,
                ContactEmail = warehouse.ContactEmail,
                ContactPhone = warehouse.ContactPhone,
                IsActive = warehouse.IsActive,
                CreatedAt = warehouse.CreatedAt,
                UpdatedAt = warehouse.UpdatedAt,
                TotalProducts = inventoryItems.Count(),
                TotalItems = inventoryItems.Sum(i => i.Quantity),
                LowStockItems = inventoryItems.Count(i => i.IsLowStock),
                InventoryItems = inventoryItems.ToList()
            };

            return Ok(result);
        }
    }
}
