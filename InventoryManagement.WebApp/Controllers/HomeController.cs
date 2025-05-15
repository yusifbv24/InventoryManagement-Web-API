using InventoryManagement.WebApp.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.WebApp.Models;

namespace InventoryManagement.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductsService _productsService;
        private readonly InventoryService _inventoryService;
        private readonly OrdersService _ordersService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ProductsService productsService,
            InventoryService inventoryService,
            OrdersService ordersService,
            ILogger<HomeController> logger)
        {
            _productsService = productsService;
            _inventoryService = inventoryService;
            _ordersService = ordersService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Get counts for dashboard
            var productsResponse = await _productsService.GetAllProductsAsync();
            var inventoryResponse = await _inventoryService.GetAllInventoryItemsAsync();
            var lowStockResponse = await _inventoryService.GetLowStockItemsAsync();
            var warehousesResponse = await _inventoryService.GetAllWarehousesAsync();
            var ordersResponse = await _ordersService.GetAllOrdersAsync();

            ViewBag.ProductCount = productsResponse.Success ? productsResponse.Data?.Count ?? 0 : 0;
            ViewBag.InventoryCount = inventoryResponse.Success ? inventoryResponse.Data?.Count ?? 0 : 0;
            ViewBag.LowStockCount = lowStockResponse.Success ? lowStockResponse.Data?.Count ?? 0 : 0;
            ViewBag.WarehouseCount = warehousesResponse.Success ? warehousesResponse.Data?.Count ?? 0 : 0;
            ViewBag.OrderCount = ordersResponse.Success ? ordersResponse.Data?.Count ?? 0 : 0;

            // Get recent orders
            if (ordersResponse.Success)
            {
                ViewBag.RecentOrders = ordersResponse.Data?
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList();
            }

            // Get low stock items
            if (lowStockResponse.Success)
            {
                ViewBag.LowStockItems = lowStockResponse.Data;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
