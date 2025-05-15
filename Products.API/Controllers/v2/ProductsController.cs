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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all products with filtering options
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] bool? isActive = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting filtered products");

            var products = await _productService.GetAllProductsAsync(cancellationToken);

            // Apply filters
            if (isActive.HasValue)
                products = products.Where(p => p.IsActive == isActive.Value);

            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice.Value);

            return Ok(products);
        }

        /// <summary>
        /// Searches for products by name, description or SKU
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(
            [FromQuery] string query,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching products: {Query}", query);

            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty" });

            var allProducts = await _productService.GetAllProductsAsync(cancellationToken);

            var matchingProducts = allProducts.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.SKU.Contains(query, StringComparison.OrdinalIgnoreCase)
            );

            return Ok(matchingProducts);
        }

        /// <summary>
        /// Batch activates multiple products
        /// </summary>
        [HttpPatch("batch-activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BatchOperationResult>> BatchActivateProducts(
            [FromBody] List<int> productIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Batch activating products: {ProductIds}", string.Join(",", productIds));

            var result = new BatchOperationResult
            {
                SuccessfulIds = new List<int>(),
                FailedIds = new Dictionary<int, string>()
            };

            foreach (var id in productIds)
            {
                try
                {
                    await _productService.ActivateProductAsync(id, cancellationToken);
                    result.SuccessfulIds.Add(id);
                }
                catch (Exception ex)
                {
                    result.FailedIds.Add(id, ex.Message);
                }
            }

            return Ok(result);
        }
    }
}
