using InventoryManagement.WebApp.Models.Products;
using InventoryManagement.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WebApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ProductsService _productsService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ProductsService productsService, ILogger<CategoriesController> logger)
        {
            _productsService = productsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _productsService.GetAllCategoriesAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return View(new List<CategoryViewModel>());
            }

            return View(response.Data);
        }
    }
}
