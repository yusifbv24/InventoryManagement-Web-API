using InventoryManagement.WebApp.Models.Products;
using InventoryManagement.WebApp.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductsService _productsService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductsService productsService, ILogger<ProductsController> logger)
        {
            _productsService = productsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _productsService.GetAllProductsAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return View(new List<ProductViewModel>());
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _productsService.GetProductByIdAsync(id);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productsService.CreateProductAsync(model);
                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Product created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", response.ErrorMessage ?? "Failed to create product.");
            }

            await PopulateDropdowns();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _productsService.GetProductByIdAsync(id);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var product = response.Data;
            var updateModel = new UpdateProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                ImageUrl = product.ImageUrl,
                Weight = product.Weight,
                Dimensions = product.Dimensions
            };

            await PopulateDropdowns();
            return View(updateModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid product ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                var response = await _productsService.UpdateProductAsync(id, model);
                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Product updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", response.ErrorMessage ?? "Failed to update product.");
            }

            await PopulateDropdowns();
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _productsService.GetProductByIdAsync(id);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _productsService.DeleteProductAsync(id);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns()
        {
            var categoriesResponse = await _productsService.GetAllCategoriesAsync();
            if (categoriesResponse.Success && categoriesResponse.Data != null)
            {
                ViewBag.Categories = new SelectList(categoriesResponse.Data, "Id", "Name");
            }

            var suppliersResponse = await _productsService.GetAllSuppliersAsync();
            if (suppliersResponse.Success && suppliersResponse.Data != null)
            {
                ViewBag.Suppliers = new SelectList(suppliersResponse.Data, "Id", "Name");
            }
        }
    }
}
