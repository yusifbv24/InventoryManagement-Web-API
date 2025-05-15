using InventoryManagement.WebApp.Models.Orders;
using InventoryManagement.WebApp.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.WebApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrdersService _ordersService;
        private readonly ProductsService _productsService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            OrdersService ordersService,
            ProductsService productsService,
            ILogger<OrdersController> logger)
        {
            _ordersService = ordersService;
            _productsService = productsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _ordersService.GetAllOrdersAsync();
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return View(new List<OrderViewModel>());
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _ordersService.GetOrderByIdAsync(id);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var historyResponse = await _ordersService.GetOrderHistoryAsync(id);
            ViewBag.OrderHistory = historyResponse.Success ? historyResponse.Data : new List<OrderStatusHistoryViewModel>();

            return View(response.Data);
        }

        public async Task<IActionResult> Create()
        {
            var productsResponse = await _productsService.GetAllProductsAsync();
            if (productsResponse.Success && productsResponse.Data != null)
            {
                ViewBag.Products = new SelectList(productsResponse.Data, "Id", "Name");
            }

            return View(new CreateOrderViewModel
            {
                Items = new List<CreateOrderItemViewModel> { new CreateOrderItemViewModel() }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Filter out any empty items
                model.Items = model.Items.Where(i => i.ProductId > 0 && i.Quantity > 0).ToList();

                if (!model.Items.Any())
                {
                    ModelState.AddModelError("", "Order must contain at least one product.");
                }
                else
                {
                    var response = await _ordersService.CreateOrderAsync(model);
                    if (response.Success)
                    {
                        TempData["SuccessMessage"] = "Order created successfully.";
                        return RedirectToAction(nameof(Details), new { id = response.Data?.Id });
                    }

                    ModelState.AddModelError("", response.ErrorMessage ?? "Failed to create order.");
                }
            }

            var productsResponse = await _productsService.GetAllProductsAsync();
            if (productsResponse.Success && productsResponse.Data != null)
            {
                ViewBag.Products = new SelectList(productsResponse.Data, "Id", "Name");
            }

            return View(model);
        }

        public async Task<IActionResult> UpdateStatus(int id)
        {
            var response = await _ordersService.GetOrderByIdAsync(id);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var order = response.Data;
            var model = new UpdateOrderStatusViewModel
            {
                OrderId = order.Id,
                NewStatus = order.Status
            };

            ViewBag.Order = order;
            ViewBag.Statuses = new SelectList(new[]
            {
                "Created", "PendingInventory", "Reserved", "Processing",
                "Shipped", "Delivered", "Cancelled", "Returned"
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateOrderStatusViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _ordersService.UpdateOrderStatusAsync(model);
                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Order status updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = model.OrderId });
                }

                ModelState.AddModelError("", response.ErrorMessage ?? "Failed to update order status.");
            }

            var orderResponse = await _ordersService.GetOrderByIdAsync(model.OrderId);
            ViewBag.Order = orderResponse.Success ? orderResponse.Data : null;

            ViewBag.Statuses = new SelectList(new[]
            {
                "Created", "PendingInventory", "Reserved", "Processing",
                "Shipped", "Delivered", "Cancelled", "Returned"
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var response = await _ordersService.CancelOrderAsync(id, reason);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = response.ErrorMessage;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
