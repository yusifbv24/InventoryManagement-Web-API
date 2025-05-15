using Asp.Versioning;
using Inventory.Application.DTOs.Warehouse;
using Inventory.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehousesController> _logger;

        public WarehousesController(IWarehouseService warehouseService, ILogger<WarehousesController> logger)
        {
            _warehouseService = warehouseService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAllWarehouses(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all warehouses");
            var warehouses = await _warehouseService.GetAllWarehousesAsync(cancellationToken);
            return Ok(warehouses);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WarehouseDto>> GetWarehouseById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting warehouse by ID: {WarehouseId}", id);
            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, cancellationToken);

            if (warehouse == null)
                return NotFound();

            return Ok(warehouse);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WarehouseDto>> CreateWarehouse([FromBody] CreateWarehouseDto createWarehouseDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating warehouse: {WarehouseName}", createWarehouseDto.Name);

            try
            {
                var warehouse = await _warehouseService.CreateWarehouseAsync(createWarehouseDto, cancellationToken);
                return CreatedAtAction(nameof(GetWarehouseById), new { id = warehouse.Id, version = "1.0" }, warehouse);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error creating warehouse");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseDto updateWarehouseDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating warehouse: {WarehouseId}", id);

            if (id != updateWarehouseDto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in request body" });

            try
            {
                await _warehouseService.UpdateWarehouseAsync(updateWarehouseDto, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Warehouse not found for update: {WarehouseId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error updating warehouse: {WarehouseId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteWarehouse(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting warehouse: {WarehouseId}", id);

            try
            {
                await _warehouseService.DeleteWarehouseAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error deleting warehouse: {WarehouseId}", id);
                return StatusCode(
                    ex.Message.Contains("not found") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    new { message = ex.Message }
                );
            }
        }

        [HttpPatch("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateWarehouse(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Activating warehouse: {WarehouseId}", id);

            try
            {
                await _warehouseService.ActivateWarehouseAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error activating warehouse: {WarehouseId}", id);
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateWarehouse(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deactivating warehouse: {WarehouseId}", id);

            try
            {
                await _warehouseService.DeactivateWarehouseAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error deactivating warehouse: {WarehouseId}", id);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
