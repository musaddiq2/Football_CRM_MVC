using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;
using FootBallOne.Models;

namespace FootBallOne.Controllers
{
    public class FBProductAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FBProductAdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ================== DASHBOARD ==================
        public async Task<IActionResult> Dashboard()
        {
            int? academyId = GetAcademyId();

            var totalProducts = await _context.FBProducts.CountAsync();
            var activeProducts = await _context.FBProducts.Where(p => p.IsActive).CountAsync();
            var lowStockProducts = await _context.FBProducts.Where(p => p.StockQuantity <= p.MinStockLevel).CountAsync();
            var outOfStockProducts = await _context.FBProducts.Where(p => p.StockQuantity == 0).CountAsync();

            var totalOrders = academyId.HasValue
                ? await _context.FBOrders.Where(o => o.AcademyId == academyId).CountAsync()
                : await _context.FBOrders.CountAsync();

            var pendingOrders = academyId.HasValue
                ? await _context.FBOrders.Where(o => o.AcademyId == academyId && o.OrderStatus == "Pending").CountAsync()
                : await _context.FBOrders.Where(o => o.OrderStatus == "Pending").CountAsync();

            var totalRevenue = academyId.HasValue
                ? await _context.FBOrders.Where(o => o.AcademyId == academyId && o.PaymentStatus == "Paid").SumAsync(o => (decimal?)o.TotalAmount) ?? 0
                : await _context.FBOrders.Where(o => o.PaymentStatus == "Paid").SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.TotalProducts = totalProducts;
            ViewBag.ActiveProducts = activeProducts;
            ViewBag.LowStockProducts = lowStockProducts;
            ViewBag.OutOfStockProducts = outOfStockProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }

        // ================== PRODUCTS ==================
        public async Task<IActionResult> Products()
        {
            var products = await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        // GET: Create Product
        public async Task<IActionResult> CreateProduct()
        {
            await PopulateCategoriesDropdown();
            return View();
        }

        // POST: Create Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(FBProduct product, IFormFile primaryImage, List<IFormFile> additionalImages)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _context.FBProducts.Add(product);
                await _context.SaveChangesAsync();

                // Handle primary image
                if (primaryImage != null && primaryImage.Length > 0)
                {
                    var imagePath = await SaveImage(primaryImage);
                    var productImage = new FBProductImage
                    {
                        ProductId = product.ProductId,
                        ImageUrl = imagePath,
                        IsPrimary = true,
                        DisplayOrder = 0,
                        AltText = product.ProductName,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.FBProductImages.Add(productImage);
                }

                // Handle additional images
                if (additionalImages != null && additionalImages.Count > 0)
                {
                    int order = 1;
                    foreach (var image in additionalImages)
                    {
                        if (image != null && image.Length > 0)
                        {
                            var imagePath = await SaveImage(image);
                            var productImage = new FBProductImage
                            {
                                ProductId = product.ProductId,
                                ImageUrl = imagePath,
                                IsPrimary = false,
                                DisplayOrder = order++,
                                AltText = product.ProductName,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.FBProductImages.Add(productImage);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Products));
            }

            await PopulateCategoriesDropdown();
            return View(product);
        }

        // GET: Edit Product
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.FBProducts
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            await PopulateCategoriesDropdown();
            return View(product);
        }

        // POST: Edit Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, FBProduct product, IFormFile primaryImage, List<IFormFile> additionalImages)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.FBProducts
                        .FirstOrDefaultAsync(p => p.ProductId == id);

                    if (existingProduct == null) return NotFound();

                    // Update product properties
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

                    // Handle new primary image
                    if (primaryImage != null && primaryImage.Length > 0)
                    {
                        var imagePath = await SaveImage(primaryImage);
                        var existingPrimary = await _context.FBProductImages
                            .FirstOrDefaultAsync(i => i.ProductId == id && i.IsPrimary);

                        if (existingPrimary != null)
                        {
                            existingPrimary.ImageUrl = imagePath;
                            existingPrimary.AltText = product.ProductName;
                        }
                        else
                        {
                            _context.FBProductImages.Add(new FBProductImage
                            {
                                ProductId = id,
                                ImageUrl = imagePath,
                                IsPrimary = true,
                                DisplayOrder = 0,
                                AltText = product.ProductName,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // Handle additional images
                    if (additionalImages != null && additionalImages.Count > 0)
                    {
                        var maxOrder = await _context.FBProductImages
                            .Where(i => i.ProductId == id)
                            .MaxAsync(i => (int?)i.DisplayOrder) ?? 0;

                        foreach (var image in additionalImages)
                        {
                            if (image != null && image.Length > 0)
                            {
                                var imagePath = await SaveImage(image);
                                _context.FBProductImages.Add(new FBProductImage
                                {
                                    ProductId = id,
                                    ImageUrl = imagePath,
                                    IsPrimary = false,
                                    DisplayOrder = ++maxOrder,
                                    AltText = product.ProductName,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Products));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                        return NotFound();
                    else
                        throw;
                }
            }

            await PopulateCategoriesDropdown();
            return View(product);
        }

        // POST: Delete Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.FBProducts
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product != null)
            {
                _context.FBProducts.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }

            return RedirectToAction(nameof(Products));
        }

        // ================== CATEGORIES ==================
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.FBCategories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        // POST: Create Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(FBCategory category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.UtcNow;
                _context.FBCategories.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category created successfully!";
            }
            return RedirectToAction(nameof(Categories));
        }

        // POST: Edit Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, string categoryName, string description, bool isActive)
        {
            var category = await _context.FBCategories.FindAsync(id);
            if (category != null)
            {
                category.CategoryName = categoryName;
                category.Description = description;
                category.IsActive = isActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category updated successfully!";
            }
            return RedirectToAction(nameof(Categories));
        }

        // POST: Delete Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.FBCategories.FindAsync(id);
            if (category != null)
            {
                var hasProducts = await _context.FBProducts.AnyAsync(p => p.CategoryId == id);
                if (hasProducts)
                {
                    TempData["ErrorMessage"] = "Cannot delete category with existing products.";
                }
                else
                {
                    _context.FBCategories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category deleted successfully!";
                }
            }
            return RedirectToAction(nameof(Categories));
        }

        // ================== ORDERS ==================
        public async Task<IActionResult> Orders()
        {
            int? academyId = GetAcademyId();

            var orders = academyId.HasValue
                ? await _context.FBOrders
                    .Where(o => o.AcademyId == academyId)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync()
                : await _context.FBOrders
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

            return View(orders);
        }

        // GET: Order Details
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null) return NotFound();

            int? academyId = GetAcademyId();

            var order = academyId.HasValue
                ? await _context.FBOrders
                    .Where(o => o.OrderId == id && o.AcademyId == academyId)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync()
                : await _context.FBOrders
                    .Where(o => o.OrderId == id)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            // Get student details
            var student = await _context.RGManagements
                .FirstOrDefaultAsync(s => s.Id == order.Id);
            ViewBag.StudentName = student?.Name ?? "Unknown";

            return View(order);
        }

        // POST: Update Order Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string orderStatus, string trackingNumber)
        {
            var order = await _context.FBOrders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = orderStatus;
                order.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(trackingNumber))
                    order.TrackingNumber = trackingNumber;

                if (orderStatus == "Shipped" && !order.ShippedAt.HasValue)
                    order.ShippedAt = DateTime.UtcNow;

                if (orderStatus == "Delivered" && !order.DeliveredAt.HasValue)
                    order.DeliveredAt = DateTime.UtcNow;

                if (orderStatus == "Cancelled" && !order.CancelledAt.HasValue)
                    order.CancelledAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Order status updated successfully!";
            }

            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        // POST: Update Payment Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int orderId, string paymentStatus)
        {
            var order = await _context.FBOrders.FindAsync(orderId);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payment status updated successfully!";
            }

            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        // ================== PRODUCT VARIANTS ==================
        public async Task<IActionResult> ProductVariants(int productId)
        {
            var product = await _context.FBProducts
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return NotFound();

            ViewBag.ProductName = product.ProductName;
            ViewBag.ProductId = productId;

            return View(product.ProductVariants);
        }

        // POST: Create Variant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVariant(FBProductVariant variant)
        {
            if (ModelState.IsValid)
            {
                variant.CreatedAt = DateTime.UtcNow;
                variant.UpdatedAt = DateTime.UtcNow;
                _context.FBProductVariants.Add(variant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Variant created successfully!";
            }

            return RedirectToAction(nameof(ProductVariants), new { productId = variant.ProductId });
        }

        // POST: Delete Variant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int id, int productId)
        {
            var variant = await _context.FBProductVariants.FindAsync(id);
            if (variant != null)
            {
                _context.FBProductVariants.Remove(variant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Variant deleted successfully!";
            }

            return RedirectToAction(nameof(ProductVariants), new { productId });
        }

        // ================== HELPER METHODS ==================
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/uploads/products/{fileName}";
        }

        private async Task PopulateCategoriesDropdown()
        {
            ViewBag.Categories = await _context.FBCategories
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();
        }

        private int? GetAcademyId()
        {
            int? academyId = HttpContext.Session.GetInt32("AcademyID");

            if (academyId == null || academyId == 0)
            {
                var adminEmail = HttpContext.Session.GetString("AdminEmail");
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    var admin = _context.Admintbl.FirstOrDefault(a => a.Email == adminEmail);
                    if (admin != null && admin.AcademyID != null)
                        academyId = admin.AcademyID;
                }
            }

            return academyId;
        }

        private bool ProductExists(int id)
        {
            return _context.FBProducts.Any(e => e.ProductId == id);
        }
    }
}