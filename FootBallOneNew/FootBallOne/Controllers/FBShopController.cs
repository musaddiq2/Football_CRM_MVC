using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;
using FootBallOne.Models;

namespace FootBallOne.Controllers
{
    public class FBShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FBShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== SHOP HOME ==================
        public async Task<IActionResult> Index(int? categoryId, string search, string sortBy)
        {
            var query = _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive);

            // Filter by category
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            // Filter by search
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ProductName.Contains(search) || p.Description.Contains(search));

            // Sort
            query = sortBy switch
            {
                "price_low" => query.OrderBy(p => p.BasePrice),
                "price_high" => query.OrderByDescending(p => p.BasePrice),
                "name" => query.OrderBy(p => p.ProductName),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
            };

            var products = await query.ToListAsync();
            var categories = await _context.FBCategories.Where(c => c.IsActive).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;

            return View(products);
        }

        // ================== PRODUCT DETAILS ==================
        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants.Where(v => v.IsActive))
                .Include(p => p.ProductReviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null) return NotFound();

            // Increment view count
            product.ViewCount++;
            await _context.SaveChangesAsync();

            // Get related products
            var relatedProducts = await _context.FBProducts
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id && p.IsActive)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        // ================== SHOPPING CART ==================
        public async Task<IActionResult> Cart()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "CC");

            var cartItems = await _context.FBShoppingCartItems
                .Where(c => c.Id == studentId)
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(c => c.Variant)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: Add to Cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int? variantId, int quantity = 1)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return Json(new { success = false, message = "Please login to add items to cart" });

            // Check if item already in cart
            var existingItem = await _context.FBShoppingCartItems
                .FirstOrDefaultAsync(c => c.Id == studentId && c.ProductId == productId && c.VariantId == variantId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var cartItem = new FBShoppingCartItem
                {
                    Id = studentId.Value,
                    ProductId = productId,
                    VariantId = variantId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.FBShoppingCartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var cartCount = await _context.FBShoppingCartItems
                .Where(c => c.Id == studentId)
                .SumAsync(c => c.Quantity);

            return Json(new { success = true, message = "Item added to cart", cartCount });
        }

        // POST: Update Cart Quantity
        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.FBShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    _context.FBShoppingCartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                    cartItem.UpdatedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Cart));
        }

        // POST: Remove from Cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.FBShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.FBShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Cart));
        }

        // ================== CHECKOUT ==================
        public async Task<IActionResult> Checkout()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "CC");

            var cartItems = await _context.FBShoppingCartItems
                .Where(c => c.Id == studentId)
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction(nameof(Cart));

            // Get student details
            var student = await _context.RGManagements.FindAsync(studentId);
            if (student == null)
                return RedirectToAction(nameof(Cart));

            // Calculate totals
            decimal subtotal = cartItems.Sum(c => c.ItemTotal);
            decimal shippingCost = 50; // Flat shipping
            decimal taxAmount = subtotal * 0.18m; // 18% GST
            decimal totalAmount = subtotal + shippingCost + taxAmount;

            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.ShippingCost = shippingCost;
            ViewBag.TaxAmount = taxAmount;
            ViewBag.TotalAmount = totalAmount;

            // Pre-fill order form with student details
            var order = new FBOrder
            {
                Id = studentId.Value,
                ContactName = student.Name ?? "",
                ContactEmail = student.Email ?? "",
                ContactPhone = student.PhoneNo ?? "",
                ShippingAddress = student.Address ?? "",
                ShippingCity = student.City ?? "",
                ShippingState = "",
                ShippingPostalCode = "",
                ShippingCountry = "India"
            };

            return View(order);
        }

        // POST: Place Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(FBOrder order)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "CC");

            // Get AcademyID from student
            var student = await _context.RGManagements.FindAsync(studentId);
            if (student == null)
                return RedirectToAction(nameof(Cart));

            var cartItems = await _context.FBShoppingCartItems
                .Where(c => c.Id == studentId)
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction(nameof(Cart));

            // Calculate totals
            decimal subtotal = cartItems.Sum(c => c.ItemTotal);
            decimal shippingCost = 50;
            decimal taxAmount = subtotal * 0.18m;
            decimal totalAmount = subtotal + shippingCost + taxAmount;

            // Create order
            order.Id = studentId.Value;
            order.AcademyId = student.AcademyID; // CRITICAL: Set AcademyID from student
            order.OrderNumber = GenerateOrderNumber();
            order.OrderStatus = OrderStatus.Pending;
            order.PaymentStatus = PaymentStatus.Pending;
            order.SubTotal = subtotal;
            order.ShippingCost = shippingCost;
            order.TaxAmount = taxAmount;
            order.TotalAmount = totalAmount;
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            _context.FBOrders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new FBOrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    VariantId = cartItem.VariantId,
                    ProductName = cartItem.Product.ProductName,
                    SKU = cartItem.Variant?.SKU ?? cartItem.Product.SKU,
                    Size = cartItem.Variant?.Size,
                    Color = cartItem.Variant?.Color,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.FinalPrice + (cartItem.Variant?.AdditionalPrice ?? 0),
                    TotalPrice = cartItem.ItemTotal,
                    CreatedAt = DateTime.UtcNow
                };
                _context.FBOrderItems.Add(orderItem);

                // Update product stock
                if (cartItem.Variant != null)
                {
                    cartItem.Variant.StockQuantity -= cartItem.Quantity;
                }
                else
                {
                    cartItem.Product.StockQuantity -= cartItem.Quantity;
                }
            }

            // Clear cart
            _context.FBShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction(nameof(OrderConfirmation), new { id = order.OrderId });
        }

        // ================== ORDER CONFIRMATION ==================
        public async Task<IActionResult> OrderConfirmation(int? id)
        {
            if (id == null) return NotFound();

            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "CC");

            var order = await _context.FBOrders
                .Where(o => o.OrderId == id && o.Id == studentId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            return View(order);
        }

        // ================== MY ORDERS ==================
        public async Task<IActionResult> MyOrders()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "CC");

            var orders = await _context.FBOrders
                .Where(o => o.Id == studentId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order Details for Student
        public async Task<IActionResult> MyOrderDetails(int? id)
        {
            if (id == null) return NotFound();

            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "CC");

            var order = await _context.FBOrders
                .Where(o => o.OrderId == id && o.Id == studentId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Cancel Order
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId, string reason)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "CC");

            var order = await _context.FBOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Id == studentId);

            if (order != null && order.OrderStatus == OrderStatus.Pending)
            {
                order.OrderStatus = OrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;
                order.CancellationReason = reason;
                order.UpdatedAt = DateTime.UtcNow;

                // Restore stock
                var orderItems = await _context.FBOrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .Include(oi => oi.Product)
                    .Include(oi => oi.Variant)
                    .ToListAsync();

                foreach (var item in orderItems)
                {
                    if (item.Variant != null)
                    {
                        item.Variant.StockQuantity += item.Quantity;
                    }
                    else
                    {
                        item.Product.StockQuantity += item.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Order cancelled successfully!";
            }

            return RedirectToAction(nameof(MyOrders));
        }

        // ================== REVIEWS ==================
        [HttpPost]
        public async Task<IActionResult> AddReview(int productId, int rating, string reviewTitle, string reviewText)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return Json(new { success = false, message = "Please login to add review" });

            // Check if student has purchased this product
            var hasPurchased = await _context.FBOrderItems
                .AnyAsync(oi => oi.ProductId == productId && oi.Order.Id == studentId && oi.Order.OrderStatus == OrderStatus.Delivered);

            var review = new FBProductReview
            {
                ProductId = productId,
                Id = studentId.Value,
                Rating = rating,
                ReviewTitle = reviewTitle,
                ReviewText = reviewText,
                IsVerifiedPurchase = hasPurchased,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FBProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Review submitted successfully! It will be visible after approval." });
        }

        // ================== CART COUNT (AJAX) ==================
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return Json(new { count = 0 });

            var count = await _context.FBShoppingCartItems
                .Where(c => c.Id == studentId)
                .SumAsync(c => c.Quantity);

            return Json(new { count });
        }

        // ================== HELPER METHODS ==================
        private string GenerateOrderNumber()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }
    }
}