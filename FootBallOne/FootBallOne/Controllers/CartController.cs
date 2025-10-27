using FootBallOne.Interfaces;
using FootBallOne.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;

        public CartController(ICartService cartService, IConfiguration configuration)
        {
            _cartService = cartService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get current user Id (was StudentId)
            var userId = HttpContext.Session.GetInt32("Id");
            var academyId = HttpContext.Session.GetInt32("AcademyID");

            // If nobody's logged in
            if (userId == null && academyId == null)
            {
                TempData["Error"] = "Please login to view your cart.";
                return RedirectToAction("Login", "CC");
            }

            // If it's an admin, redirect to products
            if (academyId != null)
            {
                return RedirectToAction("Index", "Product");
            }

            // Normal user - show cart
            var cartItems = await _cartService.GetCartItemsAsync(userId.Value);
            var subtotal = await _cartService.GetCartSubtotalAsync(userId.Value);
            var itemCount = await _cartService.GetCartItemCountAsync(userId.Value);

            var shippingCost = decimal.Parse(_configuration["ECommerce:DefaultShippingCost"] ?? "50");
            var freeShipping = decimal.Parse(_configuration["ECommerce:FreeShippingThreshold"] ?? "500");
            var taxRate = decimal.Parse(_configuration["ECommerce:TaxRate"] ?? "18");
            var actualShippingCost = subtotal >= freeShipping ? 0 : shippingCost;
            var taxAmount = subtotal * (taxRate / 100);
            var total = subtotal + actualShippingCost + taxAmount;
            var validationErrors = await _cartService.GetCartValidationErrorsAsync(userId.Value);

            var vm = new CartViewModel
            {
                CartItems = cartItems,
                Subtotal = subtotal,
                ShippingCost = actualShippingCost,
                TaxAmount = taxAmount,
                TaxRate = taxRate,
                Total = total,
                TotalItems = itemCount,
                ValidationErrors = validationErrors,
                IsValid = !validationErrors.Any(),
                FreeShippingThreshold = freeShipping
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    return Json(new { success = false, message = "Please login to add items to cart." });
                }

                if (request.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Invalid quantity." });
                }

                var result = await _cartService.AddToCartAsync(
                    userId,
                    request.ProductId,
                    request.VariantId,
                    request.Quantity
                );

                if (result)
                {
                    var itemCount = await _cartService.GetCartItemCountAsync(userId);
                    return Json(new
                    {
                        success = true,
                        message = "Product added to cart successfully!",
                        cartItemCount = itemCount
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add product. It may be out of stock." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    return Json(new { success = false, message = "Unauthorized." });
                }

                var result = await _cartService.UpdateCartItemAsync(request.CartItemId, request.Quantity);

                if (result)
                {
                    var subtotal = await _cartService.GetCartSubtotalAsync(userId);
                    var itemCount = await _cartService.GetCartItemCountAsync(userId);

                    return Json(new
                    {
                        success = true,
                        message = "Cart updated successfully.",
                        subtotal = subtotal,
                        cartItemCount = itemCount
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update cart. Item may be out of stock." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    TempData["Error"] = "Unauthorized.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _cartService.RemoveFromCartAsync(cartItemId);

                if (result)
                {
                    TempData["Success"] = "Item removed from cart.";
                }
                else
                {
                    TempData["Error"] = "Failed to remove item.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            try
            {
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    TempData["Error"] = "Unauthorized.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _cartService.ClearCartAsync(userId);

                if (result)
                {
                    TempData["Success"] = "Cart cleared successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to clear cart.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = GetCurrentUserId();

                if (userId == 0)
                {
                    return Json(new { count = 0 });
                }

                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { count = count });
            }
            catch (Exception)
            {
                return Json(new { count = 0 });
            }
        }

        private int GetCurrentUserId()
        {
            // Changed from StudentId to Id
            var userId = HttpContext.Session.GetInt32("Id");
            return userId ?? 0;
        }
    }
}