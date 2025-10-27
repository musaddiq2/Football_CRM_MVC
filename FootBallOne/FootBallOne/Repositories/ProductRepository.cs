
using FootBallOne.Data;
using FootBallOne.Interfaces;
using FootBallOne.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Repositories
{
    /// <summary>
    /// Product Repository Implementation
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================================
        // Product CRUD Operations
        // =============================================

        public async Task<FBProduct> GetByIdAsync(int productId)
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<FBProduct> GetByIdWithDetailsAsync(int productId)
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
                .Include(p => p.ProductVariants.Where(v => v.IsActive))
                .Include(p => p.ProductReviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<IEnumerable<FBProduct>> GetAllAsync()
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsPrimary))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBProduct>> GetActiveProductsAsync()
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsPrimary))
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBProduct>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsPrimary))
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBProduct>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsPrimary))
                .Where(p => p.IsActive && p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBProduct>> GetProductsByBrandAsync(string brand)
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsPrimary))
                .Where(p => p.IsActive && p.Brand == brand)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBProduct>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveProductsAsync();

            searchTerm = searchTerm.ToLower();

            return await _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsPrimary))
                .Where(p => p.IsActive &&
                    (p.ProductName.ToLower().Contains(searchTerm) ||
                     p.Description.ToLower().Contains(searchTerm) ||
                     p.Brand.ToLower().Contains(searchTerm) ||
                     p.Tags.ToLower().Contains(searchTerm) ||
                     p.SKU.ToLower().Contains(searchTerm)))
                .OrderByDescending(p => p.ViewCount)
                .ToListAsync();
        }

        public async Task<FBProduct> GetProductBySkuAsync(string sku)
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.SKU == sku);
        }

        // =============================================
        // Pagination
        // =============================================

        public async Task<(IEnumerable<FBProduct> Products, int TotalCount)> GetProductsPagedAsync(
            int page,
            int pageSize,
            int? categoryId = null,
            string searchTerm = null,
            string sortBy = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            var query = _context.FBProducts
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsPrimary))
                .Where(p => p.IsActive)
                .AsQueryable();

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.Brand.ToLower().Contains(searchTerm) ||
                    p.Tags.ToLower().Contains(searchTerm));
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= maxPrice.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.BasePrice),
                "price_desc" => query.OrderByDescending(p => p.BasePrice),
                "name_asc" => query.OrderBy(p => p.ProductName),
                "name_desc" => query.OrderByDescending(p => p.ProductName),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "popular" => query.OrderByDescending(p => p.ViewCount),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Apply pagination
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        // =============================================
        // Product Management
        // =============================================

        public async Task<FBProduct> CreateAsync(FBProduct product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.FBProducts.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<FBProduct> UpdateAsync(FBProduct product)
        {
            product.UpdatedAt = DateTime.UtcNow;

            _context.FBProducts.Update(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<bool> DeleteAsync(int productId)
        {
            var product = await _context.FBProducts.FindAsync(productId);
            if (product == null)
                return false;

            _context.FBProducts.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _context.FBProducts.FindAsync(productId);
            if (product == null)
                return false;

            product.StockQuantity = quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task IncrementViewCountAsync(int productId)
        {
            var product = await _context.FBProducts.FindAsync(productId);
            if (product != null)
            {
                product.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        // =============================================
        // Stock Management
        // =============================================

        public async Task<bool> IsInStockAsync(int productId)
        {
            var product = await _context.FBProducts.FindAsync(productId);
            return product != null && product.StockQuantity > 0;
        }

        public async Task<IEnumerable<FBProduct>> GetLowStockProductsAsync()
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity > 0 && p.StockQuantity <= p.MinStockLevel)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBProduct>> GetOutOfStockProductsAsync()
        {
            return await _context.FBProducts
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity == 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        // =============================================
        // Category Operations
        // =============================================

        public async Task<FBCategory> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.FBCategories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<IEnumerable<FBCategory>> GetAllCategoriesAsync()
        {
            return await _context.FBCategories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBCategory>> GetActiveCategoriesAsync()
        {
            return await _context.FBCategories
                .Include(c => c.ParentCategory)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBCategory>> GetMainCategoriesAsync()
        {
            return await _context.FBCategories
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBCategory>> GetSubCategoriesAsync(int parentCategoryId)
        {
            return await _context.FBCategories
                .Where(c => c.IsActive && c.ParentCategoryId == parentCategoryId)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }
    }
}