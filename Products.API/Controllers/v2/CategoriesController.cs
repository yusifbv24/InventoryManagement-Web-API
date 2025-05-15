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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            IProductService productService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Gets categories with optional active status filter
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(
            [FromQuery] bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting categories with isActive filter: {IsActive}", isActive);

            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);

            if (isActive.HasValue)
                categories = categories.Where(c => c.IsActive == isActive.Value);

            return Ok(categories);
        }

        /// <summary>
        /// Gets a category with its products
        /// </summary>
        [HttpGet("{id}/with-products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryWithProductsDto>> GetCategoryWithProducts(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting category with products: {CategoryId}", id);

            var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
            if (category == null)
                return NotFound();

            var products = await _productService.GetProductsByCategoryAsync(id, cancellationToken);

            var result = new CategoryWithProductsDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                Products = products.ToList()
            };

            return Ok(result);
        }
    }
}
