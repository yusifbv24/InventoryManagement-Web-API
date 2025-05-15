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
        /// Gets all products
        /// </summary>
        /// <returns>A collection of products</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all products");
            var products = await _productService.GetAllProductsAsync(cancellationToken);
            return Ok(products);
        }

        /// <summary>
        /// Gets a product by ID
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>The product if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);
            var product = await _productService.GetProductByIdAsync(id, cancellationToken);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        /// <summary>
        /// Gets products by category ID
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <returns>A collection of products in the specified category</returns>
        [HttpGet("by-category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting products by category ID: {CategoryId}", categoryId);
            var products = await _productService.GetProductsByCategoryAsync(categoryId, cancellationToken);
            return Ok(products);
        }

        /// <summary>
        /// Gets products by supplier ID
        /// </summary>
        /// <param name="supplierId">The supplier ID</param>
        /// <returns>A collection of products from the specified supplier</returns>
        [HttpGet("by-supplier/{supplierId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsBySupplier(int supplierId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting products by supplier ID: {SupplierId}", supplierId);
            var products = await _productService.GetProductsBySupplierAsync(supplierId, cancellationToken);
            return Ok(products);
        }

        /// <summary>
        /// Creates a new product
        /// </summary>
        /// <param name="createProductDto">The product data</param>
        /// <returns>The created product</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new product: {ProductName}", createProductDto.Name);

            try
            {
                var product = await _productService.CreateProductAsync(createProductDto, cancellationToken);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id, version = "1.0" }, product);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Bad request while creating product");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing product
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <param name="updateProductDto">The updated product data</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating product: {ProductId}", id);

            if (id != updateProductDto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in request body" });

            try
            {
                await _productService.UpdateProductAsync(updateProductDto, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Product not found for update: {ProductId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Bad request while updating product: {ProductId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a product
        /// </summary>
        /// <param name="id">The product ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting product: {ProductId}", id);

            try
            {
                await _productService.DeleteProductAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Product not found for deletion: {ProductId}", id);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Activates a product
        /// </summary>
        /// <param name="id">The product ID</param>
        [HttpPatch("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateProduct(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Activating product: {ProductId}", id);

            try
            {
                await _productService.ActivateProductAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Product not found for activation: {ProductId}", id);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivates a product
        /// </summary>
        /// <param name="id">The product ID</param>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateProduct(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deactivating product: {ProductId}", id);

            try
            {
                await _productService.DeactivateProductAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Product not found for deactivation: {ProductId}", id);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
