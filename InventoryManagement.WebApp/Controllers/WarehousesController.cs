using InventoryManagement.WebApp.Models.Inventory;
using InventoryManagement.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WebApp.Controllers
{
    public class WarehousesController : Controller
    {
        private readonly InventoryService _inventoryService;
        private readonly ILogger<WarehousesController> _logger;

        public WarehousesController(InventoryService inventoryService, ILogger<WarehousesController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _inventoryService.GetAllWarehousesAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return View(new List<WarehouseViewModel>());
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var warehousesResponse = await _inventoryService.GetAllWarehousesAsync();
            if (!warehousesResponse.Success)
            {
                TempData["ErrorMessage"] = warehousesResponse.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var warehouse = warehousesResponse.Data?.FirstOrDefault(w => w.Id == id);
            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "Warehouse not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get inventory items for this warehouse
            var inventoryResponse = await _inventoryService.GetAllInventoryItemsAsync();
            var warehouseItems = inventoryResponse.Success
                ? inventoryResponse.Data?.Where(i => i.WarehouseId == id).ToList()
                : new List<InventoryItemViewModel>();

            ViewBag.InventoryItems = warehouseItems;

            return View(warehouse);
        }
    }
}
