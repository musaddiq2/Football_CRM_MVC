using FootBallOne.Models;
using FootBallOne.Data;
using FootBallOne.Interfaces;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Services
{
    /// <summary>
    /// Product Service Implementation - Business Logic
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public ProductService(IProductRepository productRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        // =============================================
        // Product Operations
        // =============================================

        public async Task<FBProduct> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        public async Task<FBProduct> GetProductDetailsAsync(int productId)
        {
            // Increment view count
            await _productRepository.IncrementViewCountAsync(productId);

            return await _productRepository.GetByIdWithDetailsAsync(productId);
        }

        public async Task<IEnumerable<FBProduct>> GetAllProductsAsync()
        {
            return await _productRepository.GetActiveProductsAsync();
        }

        public async Task<IEnumerable<FBProduct>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _productRepository.GetFeaturedProductsAsync(count);
        }

        public async Task<IEnumerable<FBProduct>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _productRepository.GetProductsByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<FBProduct>> SearchProductsAsync(string searchTerm)
        {
            return await _productRepository.SearchProductsAsync(searchTerm);
        }

        // =============================================
        // Pagination
        // =============================================

        public async Task<ProductPagedResult> GetProductsPagedAsync(ProductFilterOptions options)
        {
            var (products, totalCount) = await _productRepository.GetProductsPagedAsync(
                options.Page,
                options.PageSize,
                options.CategoryId,
                options.SearchTerm,
                options.SortBy,
                options.MinPrice,
                options.MaxPrice
            );

            var totalPages = (int)Math.Ceiling(totalCount / (double)options.PageSize);

            return new ProductPagedResult
            {
                Products = products,
                CurrentPage = options.Page,
                PageSize = options.PageSize,
                TotalPages = totalPages,
                TotalProducts = totalCount
            };
        }

        // =============================================
        // Product Variants
        // =============================================

        public async Task<IEnumerable<FBProductVariant>> GetProductVariantsAsync(int productId)
        {
            return await _context.FBProductVariants
                .Where(v => v.ProductId == productId && v.IsActive)
                .OrderBy(v => v.Size)
                .ThenBy(v => v.Color)
                .ToListAsync();
        }

        public async Task<FBProductVariant> GetVariantByIdAsync(int variantId)
        {
            return await _context.FBProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);
        }

        // =============================================
        // Product Images
        // =============================================

        public async Task<IEnumerable<FBProductImage>> GetProductImagesAsync(int productId)
        {
            return await _context.FBProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task<FBProductImage> GetPrimaryImageAsync(int productId)
        {
            return await _context.FBProductImages
                .Where(i => i.ProductId == productId && i.IsPrimary)
                .FirstOrDefaultAsync();
        }

        // =============================================
        // Product Reviews
        // =============================================

        public async Task<IEnumerable<FBProductReview>> GetProductReviewsAsync(int productId)
        {
            return await _context.FBProductReviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var reviews = await _context.FBProductReviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<bool> AddReviewAsync(FBProductReview review)
        {
            try
            {
                review.CreatedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;
                review.IsApproved = false; // Require admin approval

                _context.FBProductReviews.Add(review);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // =============================================
        // Categories
        // =============================================

        public async Task<IEnumerable<FBCategory>> GetAllCategoriesAsync()
        {
            return await _productRepository.GetActiveCategoriesAsync();
        }

        public async Task<IEnumerable<FBCategory>> GetMainCategoriesAsync()
        {
            return await _productRepository.GetMainCategoriesAsync();
        }

        public async Task<FBCategory> GetCategoryByIdAsync(int categoryId)
        {
            return await _productRepository.GetCategoryByIdAsync(categoryId);
        }

        // =============================================
        // Stock & Availability
        // =============================================

        public async Task<bool> IsProductAvailableAsync(int productId, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null || !product.IsActive)
                return false;

            return product.StockQuantity >= quantity;
        }

        public async Task<bool> IsVariantAvailableAsync(int variantId, int quantity = 1)
        {
            var variant = await _context.FBProductVariants.FindAsync(variantId);

            if (variant == null || !variant.IsActive)
                return false;

            return variant.StockQuantity >= quantity;
        }
    }
}