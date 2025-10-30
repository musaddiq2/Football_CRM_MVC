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

        // LOGIN
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var player = _context.RGManagements
                .FirstOrDefault(p => p.Email.ToLower() == email.ToLower() && p.Password == password);

            if (player != null)
            {
                HttpContext.Session.SetInt32("StudentId", player.Id);
                HttpContext.Session.SetString("StudentName", player.Name ?? "Student");
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // SHOP INDEX
        public async Task<IActionResult> Index(int? categoryId, string search, string sortBy)
        {
            var query = _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ProductName.Contains(search) || p.Description.Contains(search));

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

        // PRODUCT DETAILS
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

            product.ViewCount++;
            await _context.SaveChangesAsync();

            var relatedProducts = await _context.FBProducts
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id && p.IsActive)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        // CART
        public async Task<IActionResult> Cart()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login");

            var cartItems = await _context.FBShoppingCartItems
                .Where(c => c.Id == studentId)
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(c => c.Variant)
                .ToListAsync();

            return View(cartItems);
        }

        // ADD TO CART
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int? variantId, int quantity = 1)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return Json(new { success = false, message = "Please login to add items to cart" });

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

        // UPDATE CART QUANTITY
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

        // REMOVE FROM CART
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

        // CHECKOUT
        public async Task<IActionResult> Checkout()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login");

            var cartItems = await _context.FBShoppingCartItems
                .Where(c => c.Id == studentId)
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction(nameof(Cart));

            var student = await _context.RGManagements.FindAsync(studentId);
            if (student == null)
                return RedirectToAction(nameof(Cart));

            decimal subtotal = cartItems.Sum(c => c.ItemTotal);
            decimal shippingCost = 50;
            decimal taxAmount = subtotal * 0.18m;
            decimal totalAmount = subtotal + shippingCost + taxAmount;

            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.ShippingCost = shippingCost;
            ViewBag.TaxAmount = taxAmount;
            ViewBag.TotalAmount = totalAmount;

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

        // PLACE ORDER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(FBOrder order)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login");

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

            // Remove ModelState validation for fields we'll set programmatically
            ModelState.Remove("OrderNumber");
            ModelState.Remove("OrderStatus");
            ModelState.Remove("PaymentStatus");
            ModelState.Remove("Id");
            ModelState.Remove("AcademyId");
            ModelState.Remove("SubTotal");
            ModelState.Remove("TotalAmount");
            ModelState.Remove("ShippingCost");
            ModelState.Remove("TaxAmount");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            decimal subtotal = cartItems.Sum(c => c.ItemTotal);
            decimal shippingCost = 50;
            decimal taxAmount = subtotal * 0.18m;
            decimal totalAmount = subtotal + shippingCost + taxAmount;

            order.Id = studentId.Value;
            order.AcademyId = student.AcademyID;
            order.OrderNumber = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
            order.OrderStatus = "Pending";
            order.PaymentStatus = "Pending";
            order.SubTotal = subtotal;
            order.ShippingCost = shippingCost;
            order.TaxAmount = taxAmount;
            order.TotalAmount = totalAmount;
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // Set defaults for nullable fields
            if (string.IsNullOrEmpty(order.ShippingCountry))
                order.ShippingCountry = "India";

            _context.FBOrders.Add(order);
            await _context.SaveChangesAsync();

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

                if (cartItem.Variant != null)
                {
                    cartItem.Variant.StockQuantity -= cartItem.Quantity;
                }
                else
                {
                    cartItem.Product.StockQuantity -= cartItem.Quantity;
                }
            }

            _context.FBShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction(nameof(OrderConfirmation), new { id = order.OrderId });
        }

        // ORDER CONFIRMATION
        public async Task<IActionResult> OrderConfirmation(int? id)
        {
            if (id == null) return NotFound();

            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login");

            var order = await _context.FBOrders
                .Where(o => o.OrderId == id && o.Id == studentId)
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            // Load products separately to handle nulls
            var productIds = order.OrderItems.Select(oi => oi.ProductId).ToList();
            var products = await _context.FBProducts
                .Where(p => productIds.Contains(p.ProductId))
                .Include(p => p.ProductImages)
                .AsNoTracking()
                .ToListAsync();

            // Attach products to order items
            foreach (var item in order.OrderItems)
            {
                item.Product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
            }

            return View(order);
        }

        // MY ORDERS
        public async Task<IActionResult> MyOrders()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login");

            var orders = new List<FBOrder>();

            var query = @"
                SELECT 
                    OrderId, Id, AcademyId,
                    ISNULL(OrderNumber, 'ORD' + CAST(OrderId AS VARCHAR)) as OrderNumber,
                    ISNULL(OrderStatus, 'Pending') as OrderStatus,
                    ISNULL(PaymentStatus, 'Pending') as PaymentStatus,
                    ISNULL(PaymentMethod, 'N/A') as PaymentMethod,
                    ISNULL(TransactionId, '') as TransactionId,
                    SubTotal, DiscountAmount, ShippingCost, TaxAmount, TotalAmount,
                    ISNULL(ShippingAddress, 'N/A') as ShippingAddress,
                    ISNULL(ShippingCity, 'N/A') as ShippingCity,
                    ISNULL(ShippingState, 'N/A') as ShippingState,
                    ISNULL(ShippingPostalCode, 'N/A') as ShippingPostalCode,
                    ISNULL(ShippingCountry, 'India') as ShippingCountry,
                    ISNULL(ContactName, 'Unknown') as ContactName,
                    ISNULL(ContactPhone, 'N/A') as ContactPhone,
                    ISNULL(ContactEmail, 'N/A') as ContactEmail,
                    ISNULL(OrderNotes, '') as OrderNotes,
                    ISNULL(TrackingNumber, '') as TrackingNumber,
                    ShippedAt, DeliveredAt, CancelledAt,
                    ISNULL(CancellationReason, '') as CancellationReason,
                    CreatedAt, UpdatedAt
                FROM [WinfoIntern].[FBOrders]
                WHERE Id = @StudentId
                ORDER BY CreatedAt DESC";

            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    var param = command.CreateParameter();
                    param.ParameterName = "@StudentId";
                    param.Value = studentId.Value;
                    command.Parameters.Add(param);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new FBOrder
                            {
                                OrderId = reader.GetInt32(0),
                                Id = reader.GetInt32(1),
                                AcademyId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                                OrderNumber = reader.GetString(3),
                                OrderStatus = reader.GetString(4),
                                PaymentStatus = reader.GetString(5),
                                PaymentMethod = reader.GetString(6),
                                TransactionId = reader.GetString(7),
                                SubTotal = reader.GetDecimal(8),
                                DiscountAmount = reader.GetDecimal(9),
                                ShippingCost = reader.GetDecimal(10),
                                TaxAmount = reader.GetDecimal(11),
                                TotalAmount = reader.GetDecimal(12),
                                ShippingAddress = reader.GetString(13),
                                ShippingCity = reader.GetString(14),
                                ShippingState = reader.GetString(15),
                                ShippingPostalCode = reader.GetString(16),
                                ShippingCountry = reader.GetString(17),
                                ContactName = reader.GetString(18),
                                ContactPhone = reader.GetString(19),
                                ContactEmail = reader.GetString(20),
                                OrderNotes = reader.GetString(21),
                                TrackingNumber = reader.GetString(22),
                                ShippedAt = reader.IsDBNull(23) ? null : reader.GetDateTime(23),
                                DeliveredAt = reader.IsDBNull(24) ? null : reader.GetDateTime(24),
                                CancelledAt = reader.IsDBNull(25) ? null : reader.GetDateTime(25),
                                CancellationReason = reader.GetString(26),
                                CreatedAt = reader.GetDateTime(27),
                                UpdatedAt = reader.GetDateTime(28)
                            });
                        }
                    }
                }
            }

            // Get order items for each order
            foreach (var order in orders)
            {
                order.OrderItems = await _context.FBOrderItems
                    .Where(oi => oi.OrderId == order.OrderId)
                    .ToListAsync();
            }

            return View(orders);
        }

        // MY ORDER DETAILS
        public async Task<IActionResult> MyOrderDetails(int? id)
        {
            if (id == null) return NotFound();

            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login");

            FBOrder order = null;

            var query = @"
                SELECT 
                    OrderId, Id, AcademyId,
                    ISNULL(OrderNumber, 'ORD' + CAST(OrderId AS VARCHAR)) as OrderNumber,
                    ISNULL(OrderStatus, 'Pending') as OrderStatus,
                    ISNULL(PaymentStatus, 'Pending') as PaymentStatus,
                    ISNULL(PaymentMethod, 'N/A') as PaymentMethod,
                    ISNULL(TransactionId, '') as TransactionId,
                    SubTotal, DiscountAmount, ShippingCost, TaxAmount, TotalAmount,
                    ISNULL(ShippingAddress, 'N/A') as ShippingAddress,
                    ISNULL(ShippingCity, 'N/A') as ShippingCity,
                    ISNULL(ShippingState, 'N/A') as ShippingState,
                    ISNULL(ShippingPostalCode, 'N/A') as ShippingPostalCode,
                    ISNULL(ShippingCountry, 'India') as ShippingCountry,
                    ISNULL(ContactName, 'Unknown') as ContactName,
                    ISNULL(ContactPhone, 'N/A') as ContactPhone,
                    ISNULL(ContactEmail, 'N/A') as ContactEmail,
                    ISNULL(OrderNotes, '') as OrderNotes,
                    ISNULL(TrackingNumber, '') as TrackingNumber,
                    ShippedAt, DeliveredAt, CancelledAt,
                    ISNULL(CancellationReason, '') as CancellationReason,
                    CreatedAt, UpdatedAt
                FROM [WinfoIntern].[FBOrders]
                WHERE OrderId = @OrderId AND Id = @StudentId";

            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    var param1 = command.CreateParameter();
                    param1.ParameterName = "@OrderId";
                    param1.Value = id.Value;
                    command.Parameters.Add(param1);

                    var param2 = command.CreateParameter();
                    param2.ParameterName = "@StudentId";
                    param2.Value = studentId.Value;
                    command.Parameters.Add(param2);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            order = new FBOrder
                            {
                                OrderId = reader.GetInt32(0),
                                Id = reader.GetInt32(1),
                                AcademyId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                                OrderNumber = reader.GetString(3),
                                OrderStatus = reader.GetString(4),
                                PaymentStatus = reader.GetString(5),
                                PaymentMethod = reader.GetString(6),
                                TransactionId = reader.GetString(7),
                                SubTotal = reader.GetDecimal(8),
                                DiscountAmount = reader.GetDecimal(9),
                                ShippingCost = reader.GetDecimal(10),
                                TaxAmount = reader.GetDecimal(11),
                                TotalAmount = reader.GetDecimal(12),
                                ShippingAddress = reader.GetString(13),
                                ShippingCity = reader.GetString(14),
                                ShippingState = reader.GetString(15),
                                ShippingPostalCode = reader.GetString(16),
                                ShippingCountry = reader.GetString(17),
                                ContactName = reader.GetString(18),
                                ContactPhone = reader.GetString(19),
                                ContactEmail = reader.GetString(20),
                                OrderNotes = reader.GetString(21),
                                TrackingNumber = reader.GetString(22),
                                ShippedAt = reader.IsDBNull(23) ? null : reader.GetDateTime(23),
                                DeliveredAt = reader.IsDBNull(24) ? null : reader.GetDateTime(24),
                                CancelledAt = reader.IsDBNull(25) ? null : reader.GetDateTime(25),
                                CancellationReason = reader.GetString(26),
                                CreatedAt = reader.GetDateTime(27),
                                UpdatedAt = reader.GetDateTime(28)
                            };
                        }
                    }
                }
            }

            if (order == null) return NotFound();

            order.OrderItems = await _context.FBOrderItems
                .Where(oi => oi.OrderId == id)
                .Include(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .ToListAsync();

            return View(order);
        }

        // CANCEL ORDER
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId, string reason)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login");

            var order = await _context.FBOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Id == studentId);

            if (order != null && order.OrderStatus == "Pending")
            {
                order.OrderStatus = "Cancelled";
                order.CancelledAt = DateTime.UtcNow;
                order.CancellationReason = reason;
                order.UpdatedAt = DateTime.UtcNow;

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

        // ADD REVIEW
        [HttpPost]
        public async Task<IActionResult> AddReview(int productId, int rating, string reviewTitle, string reviewText)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return Json(new { success = false, message = "Please login to add review" });

            var hasPurchased = await _context.FBOrderItems
                .AnyAsync(oi => oi.ProductId == productId && oi.Order.Id == studentId && oi.Order.OrderStatus == "Delivered");

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

            return Json(new { success = true, message = "Review submitted successfully!" });
        }

        // GET CART COUNT
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
    }
}