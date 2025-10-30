using FootBallOne.Data;
using FootBallOne.DTO;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        // DASHBOARD
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

        // PRODUCTS LIST
        public async Task<IActionResult> Products()
        {
            var products = await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        // CREATE PRODUCT GET
        public async Task<IActionResult> CreateProduct()
        {
            await PopulateCategoriesDropdown();
            return View();
        }

        // CREATE PRODUCT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(FBProduct product, IFormFile primaryImage, List<IFormFile> additionalImages)
        {
            try
            {
                ModelState.Remove("primaryImage");
                ModelState.Remove("additionalImages");
                ModelState.Remove("Category");
                ModelState.Remove("ProductVariants");
                ModelState.Remove("ProductImages");
                ModelState.Remove("ProductReviews");
                ModelState.Remove("ShoppingCartItems");
                ModelState.Remove("OrderItems");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    TempData["ModelErrors"] = string.Join(", ", errors);
                    await PopulateCategoriesDropdown();
                    return View(product);
                }

                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                product.ViewCount = 0;

                _context.FBProducts.Add(product);
                await _context.SaveChangesAsync();

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
            catch (Exception ex)
            {
                TempData["ModelErrors"] = $"Error: {ex.Message}";
                await PopulateCategoriesDropdown();
                return View(product);
            }
        }

        // EDIT PRODUCT GET
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

        // EDIT PRODUCT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, FBProduct product, IFormFile primaryImage, List<IFormFile> additionalImages)
        {
            if (id != product.ProductId) return NotFound();

            // REMOVE MODELSTATE entries for navigation/file properties
            ModelState.Remove("primaryImage");
            ModelState.Remove("additionalImages");
            ModelState.Remove("Category");
            ModelState.Remove("ProductVariants");
            ModelState.Remove("ProductImages");
            // ... remove other navigation properties as needed (e.g., reviews, order items)

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Get existing product and images
                    var existingProduct = await _context.FBProducts
                        .Include(p => p.ProductImages) // Important: Must include images to get the old path
                        .FirstOrDefaultAsync(p => p.ProductId == id);

                    if (existingProduct == null) return NotFound();

                    // 2. Transfer values from the posted 'product' DTO to 'existingProduct' entity
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

                    // 3. Handle Primary Image Update (CRITICAL LOGIC)
                    if (primaryImage != null && primaryImage.Length > 0)
                    {
                        var existingPrimary = existingProduct.ProductImages.FirstOrDefault(i => i.IsPrimary);

                        if (existingPrimary != null)
                        {
                            // A. Delete Old Image File from server storage
                            DeleteImage(existingPrimary.ImageUrl);

                            // B. Save New Image
                            var newImagePath = await SaveImage(primaryImage);

                            // C. Update database record
                            existingPrimary.ImageUrl = newImagePath;
                            existingPrimary.AltText = product.ProductName;
                            // Note: Update not necessary since it's an attached entity, but good practice to explicitly mark
                            _context.FBProductImages.Update(existingPrimary);
                        }
                        else
                        {
                            // Case: Product had no primary image before, but one is uploaded now
                            var newImagePath = await SaveImage(primaryImage);
                            _context.FBProductImages.Add(new FBProductImage
                            {
                                ProductId = id,
                                ImageUrl = newImagePath,
                                IsPrimary = true,
                                DisplayOrder = 0,
                                AltText = product.ProductName,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                    // 4. Handle Additional Images Addition
                    if (additionalImages != null && additionalImages.Count > 0)
                    {
                        var maxOrder = existingProduct.ProductImages.Max(i => (int?)i.DisplayOrder) ?? 0;

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

                    // 5. Save all changes
                    _context.FBProducts.Update(existingProduct); // Update the core product entity
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
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred during product update: {ex.Message}";
                }
            }

            // If ModelState is NOT valid or an exception occurred, re-populate dropdowns and return view.
            await PopulateCategoriesDropdown();
            return View(product);
        }
        // HELPER METHOD TO DELETE IMAGE FILE FROM SERVER
        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            // Remove the leading '/' from the URL to build the file path
            var fileName = imagePath.TrimStart('/');

            // Combine WebRootPath with the relative image path
            var filePath = Path.Combine(_env.WebRootPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        // DELETE PRODUCT
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

        // CATEGORIES
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.FBCategories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        // CREATE CATEGORY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(FBCategory category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
                _context.FBCategories.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category created successfully!";
            }
            return RedirectToAction(nameof(Categories));
        }

        // EDIT CATEGORY
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
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category updated successfully!";
            }
            return RedirectToAction(nameof(Categories));
        }

        // DELETE CATEGORY
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

        // ORDERS
        public async Task<IActionResult> Orders()
        {
            int? academyId = GetAcademyId();

            var orders = new List<FBOrderDTO>();

            var query = @"
                SELECT 
                    o.OrderId,
                    o.Id,
                    o.AcademyId,
                    ISNULL(o.OrderNumber, 'ORD' + CAST(o.OrderId AS VARCHAR)) as OrderNumber,
                    ISNULL(o.OrderStatus, 'Pending') as OrderStatus,
                    ISNULL(o.PaymentStatus, 'Pending') as PaymentStatus,
                    ISNULL(o.PaymentMethod, 'N/A') as PaymentMethod,
                    o.TotalAmount,
                    ISNULL(o.ContactName, 'Unknown') as ContactName,
                    ISNULL(o.ContactPhone, 'N/A') as ContactPhone,
                    ISNULL(o.ContactEmail, 'N/A') as ContactEmail,
                    o.CreatedAt,
                    (SELECT COUNT(*) FROM [WinfoIntern].[FBOrderItems] WHERE OrderId = o.OrderId) as ItemsCount
                FROM [WinfoIntern].[FBOrders] o
                " + (academyId.HasValue && academyId.Value > 0 ? "WHERE o.AcademyId = @AcademyId" : "") + @"
                ORDER BY o.CreatedAt DESC";

            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State == System.Data.ConnectionState.Closed;

            try
            {
                if (shouldCloseConnection)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    if (academyId.HasValue && academyId.Value > 0)
                    {
                        var param = command.CreateParameter();
                        param.ParameterName = "@AcademyId";
                        param.Value = academyId.Value;
                        command.Parameters.Add(param);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new FBOrderDTO
                            {
                                OrderId = reader.GetInt32(0),
                                Id = reader.GetInt32(1),
                                AcademyId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                                OrderNumber = reader.GetString(3),
                                OrderStatus = reader.GetString(4),
                                PaymentStatus = reader.GetString(5),
                                PaymentMethod = reader.GetString(6),
                                TotalAmount = reader.GetDecimal(7),
                                ContactName = reader.GetString(8),
                                ContactPhone = reader.GetString(9),
                                ContactEmail = reader.GetString(10),
                                CreatedAt = reader.GetDateTime(11),
                                ItemsCount = reader.GetInt32(12)
                            });
                        }
                    }
                }
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    connection.Close();
                }
            }

            return View(orders);
        }

        // ORDER DETAILS
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null) return NotFound();

            int? academyId = GetAcademyId();

            // Get order with raw SQL to handle NULLs
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
                WHERE OrderId = @OrderId" +
                (academyId.HasValue && academyId.Value > 0 ? " AND AcademyId = @AcademyId" : "");

            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State == System.Data.ConnectionState.Closed;

            try
            {
                if (shouldCloseConnection)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    var param1 = command.CreateParameter();
                    param1.ParameterName = "@OrderId";
                    param1.Value = id.Value;
                    command.Parameters.Add(param1);

                    if (academyId.HasValue && academyId.Value > 0)
                    {
                        var param2 = command.CreateParameter();
                        param2.ParameterName = "@AcademyId";
                        param2.Value = academyId.Value;
                        command.Parameters.Add(param2);
                    }

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
            finally
            {
                if (shouldCloseConnection)
                {
                    connection.Close();
                }
            }

            if (order == null) return NotFound();

            // Get order items with raw SQL to handle NULLs
            order.OrderItems = new List<FBOrderItem>();

            var itemsQuery = @"
                SELECT 
                    OrderItemId, OrderId, ProductId, VariantId,
                    ISNULL(ProductName, 'Unknown Product') as ProductName,
                    ISNULL(SKU, 'N/A') as SKU,
                    ISNULL(Size, '') as Size,
                    ISNULL(Color, '') as Color,
                    Quantity, UnitPrice, DiscountAmount, TotalPrice, CreatedAt
                FROM [WinfoIntern].[FBOrderItems]
                WHERE OrderId = @OrderId";

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = itemsQuery;

                    var param = command.CreateParameter();
                    param.ParameterName = "@OrderId";
                    param.Value = id.Value;
                    command.Parameters.Add(param);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            order.OrderItems.Add(new FBOrderItem
                            {
                                OrderItemId = reader.GetInt32(0),
                                OrderId = reader.GetInt32(1),
                                ProductId = reader.GetInt32(2),
                                VariantId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                ProductName = reader.GetString(4),
                                SKU = reader.GetString(5),
                                Size = reader.GetString(6),
                                Color = reader.GetString(7),
                                Quantity = reader.GetInt32(8),
                                UnitPrice = reader.GetDecimal(9),
                                DiscountAmount = reader.GetDecimal(10),
                                TotalPrice = reader.GetDecimal(11),
                                CreatedAt = reader.GetDateTime(12)
                            });
                        }
                    }
                }
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    connection.Close();
                }
            }

            var student = await _context.RGManagements.FirstOrDefaultAsync(s => s.Id == order.Id);
            ViewBag.StudentName = student?.Name ?? "Unknown";

            return View(order);
        }

        // UPDATE ORDER STATUS
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

        // UPDATE PAYMENT STATUS
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

        // PRODUCT VARIANTS
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

        // CREATE VARIANT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVariant(FBProductVariant variant)
        {
            ModelState.Remove("Product");

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

        // DELETE VARIANT
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

        // HELPER METHODS
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

        private bool ProductExists(int id)
        {
            return _context.FBProducts.Any(e => e.ProductId == id);
        }
    }
}