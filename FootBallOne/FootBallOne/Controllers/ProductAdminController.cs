using FootBallOne.Data;
using FootBallOne.Interfaces;
using FootBallOne.Models;
using FootBallOne.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Controllers
{
    public class ProductAdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductAdminController(
            IProductService productService,
            IOrderService orderService,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _orderService = orderService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // =============================================
        // Dashboard with Academy Filter
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var academyId = GetCurrentAcademyId();

                if (academyId == 0)
                {
                    TempData["Error"] = "Unauthorized access. Please login as admin.";
                    return RedirectToAction("Login", "CC");
                }

                // Get statistics filtered by academy
                var statistics = await GetAcademyOrderStatisticsAsync(academyId);

                var totalProducts = await _context.FBProducts.CountAsync();
                var activeProducts = await _context.FBProducts.CountAsync(p => p.IsActive);
                var lowStockProducts = await _context.FBProducts
                    .CountAsync(p => p.IsActive && p.StockQuantity > 0 && p.StockQuantity <= p.MinStockLevel);
                var outOfStockProducts = await _context.FBProducts
                    .CountAsync(p => p.IsActive && p.StockQuantity == 0);

                // Get recent orders for this academy only
                var recentOrders = await _context.FBOrders
                    .Include(o => o.OrderItems)
                    .Where(o => o.  AcademyId == academyId)  // FILTER BY ACADEMY
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                var viewModel = new ProductAdminDashboardViewModel
                {
                    OrderStatistics = statistics,
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    LowStockProducts = lowStockProducts,
                    OutOfStockProducts = outOfStockProducts,
                    RecentOrders = recentOrders
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load dashboard.";
                return View(new ProductAdminDashboardViewModel());
            }
        }

        // =============================================
        // Orders Management with Academy Filter
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Orders(string status)
        {
            try
            {
                var academyId = GetCurrentAcademyId();

                if (academyId == 0)
                {
                    TempData["Error"] = "Unauthorized access.";
                    return RedirectToAction("Login", "CC");
                }

                // Filter orders by academy
                var orders = await _context.FBOrders
                    .Include(o => o.OrderItems)
                    .Where(o => o.AcademyId == academyId)  // FILTER BY ACADEMY
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                if (!string.IsNullOrEmpty(status))
                {
                    orders = orders.Where(o => o.OrderStatus == status).ToList();
                }

                ViewBag.SelectedStatus = status;
                ViewBag.AcademyId = academyId;

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load orders.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            try
            {
                var academyId = GetCurrentAcademyId();

                var order = await _context.FBOrders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == id && o.AcademyId == academyId);  // FILTER BY ACADEMY

                if (order == null)
                {
                    TempData["Error"] = "Order not found or access denied.";
                    return RedirectToAction(nameof(Orders));
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
                return RedirectToAction(nameof(Orders));
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var academyId = GetCurrentAcademyId();

                // Verify order belongs to academy
                var order = await _context.FBOrders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.AcademyId == academyId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found or access denied." });
                }

                var result = await _orderService.UpdateOrderStatusAsync(orderId, status);

                if (result)
                {
                    return Json(new { success = true, message = "Order status updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update order status." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int orderId, string status)
        {
            try
            {
                var academyId = GetCurrentAcademyId();

                // Verify order belongs to academy
                var order = await _context.FBOrders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.AcademyId == academyId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found or access denied." });
                }

                var result = await _orderService.UpdatePaymentStatusAsync(orderId, status);

                if (result)
                {
                    return Json(new { success = true, message = "Payment status updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update payment status." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        // =============================================
        // Products Management
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Products(string searchTerm, int? categoryId)
        {
            try
            {
                var products = await _context.FBProducts
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages.Where(i => i.IsPrimary))
                    .AsQueryable()
                    .ToListAsync();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    products = products.Where(p =>
                        p.ProductName.ToLower().Contains(searchTerm) ||
                        p.SKU.ToLower().Contains(searchTerm) ||
                        (p.Brand != null && p.Brand.ToLower().Contains(searchTerm))).ToList();
                }

                if (categoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
                }

                var categories = await _context.FBCategories.ToListAsync();

                ViewBag.SearchTerm = searchTerm;
                ViewBag.CategoryId = categoryId;
                ViewBag.Categories = categories;

                return View(products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load products.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.FBCategories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(FBProduct product, List<IFormFile> imageFiles)
        {
            try
            {
                ModelState.Remove("Category");
                ModelState.Remove("ProductVariants");
                ModelState.Remove("ProductImages");
                ModelState.Remove("ProductReviews");
                ModelState.Remove("ShoppingCartItems");
                ModelState.Remove("OrderItems");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    TempData["Error"] = "Validation failed: " + string.Join(", ", errors);

                    ViewBag.Categories = await _context.FBCategories.Where(c => c.IsActive).ToListAsync();
                    return View(product);
                }

                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _context.FBProducts.Add(product);
                await _context.SaveChangesAsync();

                if (imageFiles != null && imageFiles.Any())
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    int displayOrder = 0;
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }

                            var productImage = new FBProductImage
                            {
                                ProductId = product.ProductId,
                                ImageUrl = "/images/products/" + uniqueFileName,  // Correct format
                                AltText = product.ProductName,
                                IsPrimary = displayOrder == 0,
                                DisplayOrder = displayOrder++,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.FBProductImages.Add(productImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create product: {ex.Message}";
                ViewBag.Categories = await _context.FBCategories.Where(c => c.IsActive).ToListAsync();
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            try
            {
                var product = await _context.FBProducts
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Products));
                }

                ViewBag.Categories = await _context.FBCategories.Where(c => c.IsActive).ToListAsync();
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load product.";
                return RedirectToAction(nameof(Products));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(FBProduct product, List<IFormFile> imageFiles)
        {
            try
            {
                ModelState.Remove("Category");
                ModelState.Remove("ProductVariants");
                ModelState.Remove("ProductImages");
                ModelState.Remove("ProductReviews");
                ModelState.Remove("ShoppingCartItems");
                ModelState.Remove("OrderItems");

                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _context.FBCategories.Where(c => c.IsActive).ToListAsync();
                    return View(product);
                }

                var existingProduct = await _context.FBProducts.FindAsync(product.ProductId);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Products));
                }

                existingProduct.ProductName = product.ProductName;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.SKU = product.SKU;
                existingProduct.Description = product.Description;
                existingProduct.Brand = product.Brand;
                existingProduct.BasePrice = product.BasePrice;
                existingProduct.DiscountPercentage = product.DiscountPercentage;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.MinStockLevel = product.MinStockLevel;
                existingProduct.Weight = product.Weight;
                existingProduct.Dimensions = product.Dimensions;
                existingProduct.Tags = product.Tags;
                existingProduct.IsActive = product.IsActive;
                existingProduct.IsFeatured = product.IsFeatured;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                // Handle new image uploads
                if (imageFiles != null && imageFiles.Any())
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var maxDisplayOrder = await _context.FBProductImages
                        .Where(i => i.ProductId == product.ProductId)
                        .MaxAsync(i => (int?)i.DisplayOrder) ?? -1;

                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }

                            var productImage = new FBProductImage
                            {
                                ProductId = product.ProductId,
                                ImageUrl = "/images/products/" + uniqueFileName,
                                AltText = product.ProductName,
                                IsPrimary = false,
                                DisplayOrder = ++maxDisplayOrder,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.FBProductImages.Add(productImage);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to update product.";
                ViewBag.Categories = await _context.FBCategories.Where(c => c.IsActive).ToListAsync();
                return View(product);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.FBProducts
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Products));
                }

                // Delete associated images from disk
                foreach (var image in product.ProductImages)
                {
                    DeleteImageFile(image.ImageUrl);
                }

                _context.FBProducts.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product deleted successfully!";
                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete product. It may be referenced in orders.";
                return RedirectToAction(nameof(Products));
            }
        }

        // =============================================
        // Categories Management
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.FBCategories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCategory()
        {
            ViewBag.ParentCategories = await _context.FBCategories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(FBCategory category, IFormFile imageFile)
        {
            try
            {
                ModelState.Remove("ParentCategory");
                ModelState.Remove("SubCategories");
                ModelState.Remove("Products");
                ModelState.Remove("ImageUrl");

                if (imageFile != null && imageFile.Length > 0)
                {
                    category.ImageUrl = await SaveImageFile(imageFile, "categories");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    TempData["Error"] = "Validation failed: " + string.Join(", ", errors);

                    ViewBag.ParentCategories = await _context.FBCategories
                        .Where(c => c.ParentCategoryId == null)
                        .ToListAsync();
                    return View(category);
                }

                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                _context.FBCategories.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create category: {ex.Message}";
                ViewBag.ParentCategories = await _context.FBCategories
                    .Where(c => c.ParentCategoryId == null)
                    .ToListAsync();
                return View(category);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.FBCategories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            ViewBag.ParentCategories = await _context.FBCategories
                .Where(c => c.ParentCategoryId == null && c.CategoryId != id)
                .ToListAsync();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(FBCategory category, IFormFile imageFile)
        {
            try
            {
                var existingCategory = await _context.FBCategories.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

                if (existingCategory == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction(nameof(Categories));
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    string newImageUrl = await SaveImageFile(imageFile, "categories");

                    if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                    {
                        DeleteImageFile(existingCategory.ImageUrl);
                    }

                    category.ImageUrl = newImageUrl;
                }
                else
                {
                    category.ImageUrl = existingCategory.ImageUrl;
                }

                ModelState.Remove("ParentCategory");
                ModelState.Remove("SubCategories");
                ModelState.Remove("Products");

                if (!ModelState.IsValid)
                {
                    ViewBag.ParentCategories = await _context.FBCategories
                        .Where(c => c.ParentCategoryId == null && c.CategoryId != category.CategoryId)
                        .ToListAsync();
                    return View(category);
                }

                category.UpdatedAt = DateTime.UtcNow;
                category.CreatedAt = existingCategory.CreatedAt;

                _context.FBCategories.Update(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to update category.";
                ViewBag.ParentCategories = await _context.FBCategories
                    .Where(c => c.ParentCategoryId == null && c.CategoryId != category.CategoryId)
                    .ToListAsync();
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.FBCategories.FindAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction(nameof(Categories));
                }

                DeleteImageFile(category.ImageUrl);

                _context.FBCategories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Category deleted successfully!";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete category. It may contain products.";
                return RedirectToAction(nameof(Categories));
            }
        }

        // =============================================
        // Helper Methods
        // =============================================
        private int GetCurrentAcademyId()
        {
            var academyId = HttpContext.Session.GetInt32("AcademyID");
            return academyId ?? 0;
        }

        private async Task<OrderStatistics> GetAcademyOrderStatisticsAsync(int academyId)
        {
            var allOrders = await _context.FBOrders
                .Where(o => o.AcademyId == academyId)  // FILTER BY ACADEMY
                .ToListAsync();

            return new OrderStatistics
            {
                TotalOrders = allOrders.Count,
                PendingOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Pending),
                ProcessingOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Processing),
                DeliveredOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Delivered),
                CancelledOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Cancelled),
                TotalRevenue = allOrders.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.TotalAmount),
                AverageOrderValue = allOrders.Any() ? allOrders.Average(o => o.TotalAmount) : 0
            };
        }

        private async Task<string> SaveImageFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", folder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/images/{folder}/{uniqueFileName}";
        }

        private void DeleteImageFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            string absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));

            if (System.IO.File.Exists(absolutePath))
            {
                try
                {
                    System.IO.File.Delete(absolutePath);
                }
                catch (Exception)
                {
                    // Log error in production
                }
            }
        }
    }
}