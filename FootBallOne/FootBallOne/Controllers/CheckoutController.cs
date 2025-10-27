using FootBallOne.Interfaces;
using FootBallOne.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Controllers
{
    /// <summary>
    /// Checkout Controller - Handles checkout process and order placement
    /// </summary>
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            IConfiguration configuration)
        {
            _cartService = cartService;
            _orderService = orderService;
            _configuration = configuration;
        }

        // =============================================
        // GET: /Checkout
        // Checkout Page
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var studentId = GetCurrentStudentId();

                //if (studentId == 0)
                //{
                //    TempData["Error"] = "Please login to proceed with checkout.";
                //    return RedirectToAction("Login", "Account");
                //}

                // Get cart items
                var cartItems = await _cartService.GetCartItemsAsync(studentId);

                if (!cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                // Validate cart
                var validationErrors = await _cartService.GetCartValidationErrorsAsync(studentId);
                if (validationErrors.Any())
                {
                    TempData["Error"] = "Please resolve cart issues before checkout.";
                    return RedirectToAction("Index", "Cart");
                }

                // Calculate totals
                var subtotal = await _cartService.GetCartSubtotalAsync(studentId);
                var itemCount = await _cartService.GetCartItemCountAsync(studentId);

                // Get settings
                var shippingCost = decimal.Parse(_configuration["ECommerce:DefaultShippingCost"] ?? "50");
                var freeShippingThreshold = decimal.Parse(_configuration["ECommerce:FreeShippingThreshold"] ?? "500");
                var taxRate = decimal.Parse(_configuration["ECommerce:TaxRate"] ?? "18");

                // Calculate shipping
                var actualShippingCost = subtotal >= freeShippingThreshold ? 0 : shippingCost;

                // Calculate tax
                var taxAmount = subtotal * (taxRate / 100);

                // Calculate total
                var total = subtotal + actualShippingCost + taxAmount;

                var viewModel = new CheckoutViewModel
                {
                    CartItems = cartItems,
                    Subtotal = subtotal,
                    ShippingCost = actualShippingCost,
                    TaxAmount = taxAmount,
                    TaxRate = taxRate,
                    Total = total,
                    TotalItems = itemCount,

                    // Pre-fill with user info if available
                    // TODO: Get from user profile
                    ShippingCountry = "India"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load checkout page.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // =============================================
        // POST: /Checkout/PlaceOrder
        // Process Order
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            try
            {
                var studentId = GetCurrentStudentId();

                //if (studentId == 0)
                //{
                //    TempData["Error"] = "Please login to place an order.";
                //    return RedirectToAction("Login", "Account");
                //}

                if (!ModelState.IsValid)
                {
                    // Reload cart items for display
                    model.CartItems = await _cartService.GetCartItemsAsync(studentId);
                    return View("Index", model);
                }

                // Create checkout info
                var checkoutInfo = new CheckoutInfo
                {
                    ShippingAddress = model.ShippingAddress,
                    ShippingCity = model.ShippingCity,
                    ShippingState = model.ShippingState,
                    ShippingPostalCode = model.ShippingPostalCode,
                    ShippingCountry = model.ShippingCountry,
                    ContactName = model.ContactName,
                    ContactPhone = model.ContactPhone,
                    ContactEmail = model.ContactEmail,
                    PaymentMethod = model.PaymentMethod,
                    OrderNotes = model.OrderNotes,
                    TransactionId = GenerateTransactionId() // Generate mock transaction ID
                };

                // Create order
                var order = await _orderService.CreateOrderAsync(studentId, checkoutInfo);

                if (order != null)
                {
                    TempData["Success"] = "Order placed successfully!";
                    return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
                }
                else
                {
                    TempData["Error"] = "Failed to place order. Please try again.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // =============================================
        // GET: /Checkout/Confirmation/{orderNumber}
        // Order Confirmation Page
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Confirmation(string orderNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(orderNumber))
                {
                    return RedirectToAction("Index", "Home");
                }

                var order = await _orderService.GetOrderByNumberAsync(orderNumber);

                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("Index", "Home");
                }

                var orderItems = await _orderService.GetOrderItemsAsync(order.OrderId);

                var viewModel = new OrderConfirmationViewModel
                {
                    Order = order,
                    OrderItems = orderItems,
                    IsSuccess = true,
                    Message = "Your order has been placed successfully!"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load order confirmation.";
                return RedirectToAction("Index", "Home");
            }
        }

        // =============================================
        // Helper Methods
        // =============================================

        private int GetCurrentStudentId()
        {
            // TODO: Replace with actual authentication logic
            // Example:
            // if (User.Identity.IsAuthenticated)
            // {
            //     var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "StudentId");
            //     if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int studentId))
            //     {
            //         return studentId;
            //     }
            // }

            // For testing, return a hardcoded value or 0
            return 0;
        }

        private string GenerateTransactionId()
        {
            // Generate mock transaction ID
            // In production, this would come from payment gateway
            return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
        }
    }
}