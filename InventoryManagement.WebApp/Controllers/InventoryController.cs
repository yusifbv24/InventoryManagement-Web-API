using InventoryManagement.WebApp.Models.Inventory;
using InventoryManagement.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WebApp.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryService _inventoryService;
        private readonly ProductsService _productsService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            InventoryService inventoryService,
            ProductsService productsService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _productsService = productsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _inventoryService.GetAllInventoryItemsAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return View(new List<InventoryItemViewModel>());
            }

            return View(response.Data);
        }

        public async Task<IActionResult> LowStock()
        {
            var response = await _inventoryService.GetLowStockItemsAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return View(new List<InventoryItemViewModel>());
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Adjust(int id)
        {
            var response = await _inventoryService.GetAllInventoryItemsAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var inventoryItem = response.Data?.FirstOrDefault(i => i.Id == id);
            if (inventoryItem == null)
            {
                TempData["ErrorMessage"] = "Inventory item not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AdjustInventoryViewModel
            {
                InventoryItemId = inventoryItem.Id,
                IsAddition = true,
                Quantity = 0,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            ViewBag.InventoryItem = inventoryItem;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(AdjustInventoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _inventoryService.AdjustInventoryAsync(model);
                if (response.Success)
                {
                    TempData["SuccessMessage"] = $"Inventory {(model.IsAddition ? "increased" : "decreased")} successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", response.ErrorMessage ?? "Failed to adjust inventory.");
            }

            // Reload inventory item for the view
            var inventoryResponse = await _inventoryService.GetAllInventoryItemsAsync();
            var inventoryItem = inventoryResponse.Success ? inventoryResponse.Data?.FirstOrDefault(i => i.Id == model.InventoryItemId) : null;
            ViewBag.InventoryItem = inventoryItem;

            return View(model);
        }
    }
}
