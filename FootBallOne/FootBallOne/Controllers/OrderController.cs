using FootBallOne.Interfaces;
using FootBallOne.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Controllers
{
    /// <summary>
    /// Order Controller - View order history and details
    /// </summary>
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // =============================================
        // GET: /Order or /Order/Index
        // Order History - List of all orders
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var studentId = GetCurrentStudentId();

                //if (studentId == 0)
                //{
                //    TempData["Error"] = "Please login to view your orders.";
                //    return RedirectToAction("Login", "Account");
                //}

                var orders = await _orderService.GetOrdersByStudentAsync(studentId);
                var totalOrders = await _orderService.GetOrderCountByStudentAsync(studentId);
                var totalSpent = await _orderService.GetTotalSpentByStudentAsync(studentId);

                var viewModel = new OrderHistoryViewModel
                {
                    Orders = orders,
                    TotalOrders = totalOrders,
                    TotalSpent = totalSpent
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load order history.";
                return View(new OrderHistoryViewModel());
            }
        }

        // =============================================
        // GET: /Order/Details/5
        // Order Details Page
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var studentId = GetCurrentStudentId();

                //if (studentId == 0)
                //{
                //    TempData["Error"] = "Please login to view order details.";
                //    return RedirectToAction("Login", "Account");
                //}

                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Security check - ensure order belongs to current student
                if (order.Id != studentId)
                {
                    TempData["Error"] = "Unauthorized access.";
                    return RedirectToAction(nameof(Index));
                }

                var orderItems = await _orderService.GetOrderItemsAsync(id);

                var viewModel = new OrderDetailsViewModel
                {
                    Order = order,
                    OrderItems = orderItems
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load order details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // POST: /Order/Cancel
        // Cancel Order
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int orderId, string reason)
        {
            try
            {
                var studentId = GetCurrentStudentId();

                if (studentId == 0)
                {
                    return Json(new { success = false, message = "Unauthorized." });
                }

                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (order == null || order.Id != studentId)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                var result = await _orderService.CancelOrderAsync(orderId, reason);

                if (result)
                {
                    return Json(new { success = true, message = "Order cancelled successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to cancel order. Order may have already been shipped." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        // =============================================
        // Helper Methods
        // =============================================

        private int GetCurrentStudentId()
        {
            // TODO: Replace with actual authentication logic
            return 0;
        }
    }
}