
using FootBallOne.Interfaces;
using FootBallOne.Models;
using FootBallOne.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Controllers
{
    /// <summary>
    /// Product Controller - Handles product catalog, details, search
    /// </summary>
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private const int DEFAULT_PAGE_SIZE = 12;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // =============================================
        // GET: /Product or /Product/Index
        // Product Catalog with filters and pagination
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Index(
            int? categoryId,
            string searchTerm,
            string sortBy = "newest",
            int page = 1,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            try
            {
                // Get paginated products
                var filterOptions = new ProductFilterOptions
                {
                    Page = page,
                    PageSize = DEFAULT_PAGE_SIZE,
                    CategoryId = categoryId,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                var pagedResult = await _productService.GetProductsPagedAsync(filterOptions);

                // Get categories for filter dropdown
                var categories = await _productService.GetAllCategoriesAsync();

                // Get unique brands for filter
                var allProducts = await _productService.GetAllProductsAsync();
                var brands = allProducts
                    .Where(p => !string.IsNullOrEmpty(p.Brand))
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToList();

                var viewModel = new ProductIndexViewModel
                {
                    Products = pagedResult.Products,
                    Categories = categories,
                    Brands = brands,
                    CurrentPage = pagedResult.CurrentPage,
                    TotalPages = pagedResult.TotalPages,
                    PageSize = pagedResult.PageSize,
                    TotalProducts = pagedResult.TotalProducts,
                    SelectedCategoryId = categoryId,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load products. Please try again.";
                return View(new ProductIndexViewModel());
            }
        }

        // =============================================
        // GET: /Product/Details/5
        // Product Details Page
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetProductDetailsAsync(id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get variants
                var variants = await _productService.GetProductVariantsAsync(id);

                // Get images
                var images = await _productService.GetProductImagesAsync(id);

                // Get reviews
                var reviews = await _productService.GetProductReviewsAsync(id);
                var averageRating = await _productService.GetAverageRatingAsync(id);

                // Get related products (same category)
                var relatedProducts = await _productService.GetProductsByCategoryAsync(product.CategoryId);
                relatedProducts = relatedProducts
                    .Where(p => p.ProductId != id)
                    .Take(4)
                    .ToList();

                // Check stock availability
                var isInStock = await _productService.IsProductAvailableAsync(id);

                var viewModel = new ProductDetailsViewModel
                {
                    Product = product,
                    Variants = variants,
                    Images = images,
                    Reviews = reviews,
                    AverageRating = averageRating,
                    TotalReviews = reviews.Count(),
                    RelatedProducts = relatedProducts,
                    IsInStock = isInStock,
                    StockStatus = product.StockStatus
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // GET: /Product/Category/5
        // Products by Category
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Category(
            int id,
            string sortBy = "newest",
            int page = 1,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            try
            {
                var category = await _productService.GetCategoryByIdAsync(id);

                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get paginated products for this category
                var filterOptions = new ProductFilterOptions
                {
                    Page = page,
                    PageSize = DEFAULT_PAGE_SIZE,
                    CategoryId = id,
                    SortBy = sortBy,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                var pagedResult = await _productService.GetProductsPagedAsync(filterOptions);

                // Get subcategories if any
                var allCategories = await _productService.GetAllCategoriesAsync();
                var subCategories = allCategories.Where(c => c.ParentCategoryId == id).ToList();

                var viewModel = new CategoryProductsViewModel
                {
                    Category = category,
                    SubCategories = subCategories,
                    Products = pagedResult.Products,
                    CurrentPage = pagedResult.CurrentPage,
                    TotalPages = pagedResult.TotalPages,
                    PageSize = pagedResult.PageSize,
                    TotalProducts = pagedResult.TotalProducts,
                    SortBy = sortBy,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load category products.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // GET: /Product/Search?term=football
        // Search Products
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Search(
            string term,
            int? categoryId,
            string sortBy = "relevance",
            int page = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return RedirectToAction(nameof(Index));
                }

                // Get paginated search results
                var filterOptions = new ProductFilterOptions
                {
                    Page = page,
                    PageSize = DEFAULT_PAGE_SIZE,
                    SearchTerm = term,
                    CategoryId = categoryId,
                    SortBy = sortBy
                };

                var pagedResult = await _productService.GetProductsPagedAsync(filterOptions);

                // Get categories for filter
                var categories = await _productService.GetAllCategoriesAsync();

                var viewModel = new SearchResultsViewModel
                {
                    SearchTerm = term,
                    Products = pagedResult.Products,
                    Categories = categories,
                    CurrentPage = pagedResult.CurrentPage,
                    TotalPages = pagedResult.TotalPages,
                    PageSize = pagedResult.PageSize,
                    TotalProducts = pagedResult.TotalProducts,
                    SelectedCategoryId = categoryId,
                    SortBy = sortBy
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Search failed. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // POST: /Product/AddReview
        // Add Product Review
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(AddReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields.";
                    return RedirectToAction(nameof(Details), new { id = model.ProductId });
                }

                // TODO: Get current logged-in student ID from your authentication system
                // For now, using a placeholder - replace with actual student ID
                var studentId = GetCurrentStudentId();

                if (studentId == 0)
                {
                    TempData["Error"] = "You must be logged in to submit a review.";
                    return RedirectToAction(nameof(Details), new { id = model.ProductId });
                }

                var review = new FBProductReview
                {
                    ProductId = model.ProductId,
                    Id = studentId,
                    Rating = model.Rating,
                    ReviewTitle = model.ReviewTitle,
                    ReviewText = model.ReviewText,
                    IsVerifiedPurchase = false, // TODO: Check if student purchased this product
                    IsApproved = false // Requires admin approval
                };

                var result = await _productService.AddReviewAsync(review);

                if (result)
                {
                    TempData["Success"] = "Thank you! Your review has been submitted and is pending approval.";
                }
                else
                {
                    TempData["Error"] = "Failed to submit review. Please try again.";
                }

                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while submitting your review.";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
        }

        // =============================================
        // Helper Methods
        // =============================================

        /// <summary>
        /// Get current logged-in student ID
        /// TODO: Replace with your actual authentication logic
        /// </summary>
        private int GetCurrentStudentId()
        {
            // Example implementation - replace with your actual logic
            // if (User.Identity.IsAuthenticated)
            // {
            //     var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "StudentId");
            //     if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int studentId))
            //     {
            //         return studentId;
            //     }
            // }
            return 0; // Return 0 if not authenticated
        }
    }
}