
using FootBallOne.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootBallOne.Interfaces
{
    /// <summary>
    /// Product Repository Interface
    /// </summary>
    public interface IProductRepository
    {
        // Product CRUD Operations
        Task<FBProduct> GetByIdAsync(int productId);
        Task<FBProduct> GetByIdWithDetailsAsync(int productId);
        Task<IEnumerable<FBProduct>> GetAllAsync();
        Task<IEnumerable<FBProduct>> GetActiveProductsAsync();
        Task<IEnumerable<FBProduct>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<FBProduct>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<FBProduct>> GetProductsByBrandAsync(string brand);
        Task<IEnumerable<FBProduct>> SearchProductsAsync(string searchTerm);
        Task<FBProduct> GetProductBySkuAsync(string sku);

        // Pagination
        Task<(IEnumerable<FBProduct> Products, int TotalCount)> GetProductsPagedAsync(
            int page,
            int pageSize,
            int? categoryId = null,
            string searchTerm = null,
            string sortBy = null,
            decimal? minPrice = null,
            decimal? maxPrice = null);

        // Product Management
        Task<FBProduct> CreateAsync(FBProduct product);
        Task<FBProduct> UpdateAsync(FBProduct product);
        Task<bool> DeleteAsync(int productId);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task IncrementViewCountAsync(int productId);

        // Stock Management
        Task<bool> IsInStockAsync(int productId);
        Task<IEnumerable<FBProduct>> GetLowStockProductsAsync();
        Task<IEnumerable<FBProduct>> GetOutOfStockProductsAsync();

        // Category Operations
        Task<FBCategory> GetCategoryByIdAsync(int categoryId);
        Task<IEnumerable<FBCategory>> GetAllCategoriesAsync();
        Task<IEnumerable<FBCategory>> GetActiveCategoriesAsync();
        Task<IEnumerable<FBCategory>> GetMainCategoriesAsync();
        Task<IEnumerable<FBCategory>> GetSubCategoriesAsync(int parentCategoryId);
    }
}