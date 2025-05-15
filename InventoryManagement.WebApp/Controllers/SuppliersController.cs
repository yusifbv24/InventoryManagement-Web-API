using InventoryManagement.WebApp.Models.Products;
using InventoryManagement.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WebApp.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ProductsService _productsService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(ProductsService productsService, ILogger<SuppliersController> logger)
        {
            _productsService = productsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _productsService.GetAllSuppliersAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return View(new List<SupplierViewModel>());
            }

            return View(response.Data);
        }
    }
}
