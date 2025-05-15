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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <returns>A collection of categories</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all categories");
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
            return Ok(categories);
        }

        /// <summary>
        /// Gets a category by ID
        /// </summary>
        /// <param name="id">The category ID</param>
        /// <returns>The category if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting category by ID: {CategoryId}", id);
            var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        /// <summary>
        /// Creates a new category
        /// </summary>
        /// <param name="createCategoryDto">The category data</param>
        /// <returns>The created category</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new category: {CategoryName}", createCategoryDto.Name);

            try
            {
                var category = await _categoryService.CreateCategoryAsync(createCategoryDto, cancellationToken);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id, version = "1.0" }, category);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Bad request while creating category");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing category
        /// </summary>
        /// <param name="id">The category ID</param>
        /// <param name="updateCategoryDto">The updated category data</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating category: {CategoryId}", id);

            if (id != updateCategoryDto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in request body" });

            try
            {
                await _categoryService.UpdateCategoryAsync(updateCategoryDto, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Category not found for update: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Bad request while updating category: {CategoryId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a category
        /// </summary>
        /// <param name="id">The category ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting category: {CategoryId}", id);

            try
            {
                await _categoryService.DeleteCategoryAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Category not found for deletion: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Bad request while deleting category: {CategoryId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Activates a category
        /// </summary>
        /// <param name="id">The category ID</param>
        [HttpPatch("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateCategory(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Activating category: {CategoryId}", id);

            try
            {
                await _categoryService.ActivateCategoryAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Category not found for activation: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivates a category
        /// </summary>
        /// <param name="id">The category ID</param>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateCategory(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deactivating category: {CategoryId}", id);

            try
            {
                await _categoryService.DeactivateCategoryAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Category not found for deactivation: {CategoryId}", id);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
