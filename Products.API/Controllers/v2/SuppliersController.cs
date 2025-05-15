using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(
            ISupplierService supplierService,
            IProductService productService,
            ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService;
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Gets suppliers with name filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers(
            [FromQuery] string? nameFilter = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting suppliers with name filter: {NameFilter}", nameFilter);

            var suppliers = await _supplierService.GetAllSuppliersAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(nameFilter))
                suppliers = suppliers.Where(s => s.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));

            return Ok(suppliers);
        }

        /// <summary>
        /// Gets a summary of supplier products
        /// </summary>
        [HttpGet("{id}/products-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierProductsSummary>> GetSupplierProductsSummary(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting supplier products summary: {SupplierId}", id);

            var supplier = await _supplierService.GetSupplierByIdAsync(id, cancellationToken);
            if (supplier == null)
                return NotFound();

            var products = await _productService.GetProductsBySupplierAsync(id, cancellationToken);

            var result = new SupplierProductsSummary
            {
                SupplierId = supplier.Id,
                SupplierName = supplier.Name,
                TotalProducts = products.Count(),
                TotalValue = products.Sum(p => p.Price),
                AveragePrice = products.Any() ? products.Average(p => p.Price) : 0,
                HighestPricedProduct = products.OrderByDescending(p => p.Price).FirstOrDefault()?.Name ?? "None",
                ProductCountByCategory = products
                    .GroupBy(p => p.CategoryName)
                    .Select(g => new CategoryCount { CategoryName = g.Key, Count = g.Count() })
                    .ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Updates only the contact information of a supplier
        /// </summary>
        [HttpPatch("{id}/contact-info")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateContactInfo(
            int id,
            [FromBody] ContactInfoUpdate contactUpdate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating supplier contact info: {SupplierId}", id);

            var supplier = await _supplierService.GetSupplierByIdAsync(id, cancellationToken);
            if (supplier == null)
                return NotFound();

            // Create update DTO with current values but updated contact info
            var updateDto = new UpdateSupplierDto
            {
                Id = id,
                Name = supplier.Name,
                ContactName = contactUpdate.ContactName,
                Email = contactUpdate.Email,
                Phone = contactUpdate.Phone,
                Address = supplier.Address // Keep the original address
            };

            try
            {
                await _supplierService.UpdateSupplierAsync(updateDto, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
