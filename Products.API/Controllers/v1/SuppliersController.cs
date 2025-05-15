using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all suppliers
        /// </summary>
        /// <returns>A collection of suppliers</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAllSuppliers(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all suppliers");
            var suppliers = await _supplierService.GetAllSuppliersAsync(cancellationToken);
            return Ok(suppliers);
        }

        /// <summary>
        /// Gets a supplier by ID
        /// </summary>
        /// <param name="id">The supplier ID</param>
        /// <returns>The supplier if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierDto>> GetSupplierById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting supplier by ID: {SupplierId}", id);
            var supplier = await _supplierService.GetSupplierByIdAsync(id, cancellationToken);

            if (supplier == null)
                return NotFound();

            return Ok(supplier);
        }

        /// <summary>
        /// Creates a new supplier
        /// </summary>
        /// <param name="createSupplierDto">The supplier data</param>
        /// <returns>The created supplier</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierDto createSupplierDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new supplier: {SupplierName}", createSupplierDto.Name);

            try
            {
                var supplier = await _supplierService.CreateSupplierAsync(createSupplierDto, cancellationToken);
                return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.Id, version = "1.0" }, supplier);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Bad request while creating supplier");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing supplier
        /// </summary>
        /// <param name="id">The supplier ID</param>
        /// <param name="updateSupplierDto">The updated supplier data</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto updateSupplierDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating supplier: {SupplierId}", id);

            if (id != updateSupplierDto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in request body" });

            try
            {
                await _supplierService.UpdateSupplierAsync(updateSupplierDto, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Supplier not found for update: {SupplierId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Bad request while updating supplier: {SupplierId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a supplier
        /// </summary>
        /// <param name="id">The supplier ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSupplier(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting supplier: {SupplierId}", id);

            try
            {
                await _supplierService.DeleteSupplierAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Supplier not found for deletion: {SupplierId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Bad request while deleting supplier: {SupplierId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Activates a supplier
        /// </summary>
        /// <param name="id">The supplier ID</param>
        [HttpPatch("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateSupplier(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Activating supplier: {SupplierId}", id);

            try
            {
                await _supplierService.ActivateSupplierAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Supplier not found for activation: {SupplierId}", id);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivates a supplier
        /// </summary>
        /// <param name="id">The supplier ID</param>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateSupplier(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deactivating supplier: {SupplierId}", id);

            try
            {
                await _supplierService.DeactivateSupplierAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Supplier not found for deactivation: {SupplierId}", id);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
