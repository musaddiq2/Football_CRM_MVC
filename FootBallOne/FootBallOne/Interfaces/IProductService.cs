
using FootBallOne.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootBallOne.Interfaces
{
    /// <summary>
    /// Product Service Interface - Business Logic Layer
    /// </summary>
    public interface IProductService
    {
        // Product Operations
        Task<FBProduct> GetProductByIdAsync(int productId);
        Task<FBProduct> GetProductDetailsAsync(int productId);
        Task<IEnumerable<FBProduct>> GetAllProductsAsync();
        Task<IEnumerable<FBProduct>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<FBProduct>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<FBProduct>> SearchProductsAsync(string searchTerm);

        // Pagination
        Task<ProductPagedResult> GetProductsPagedAsync(ProductFilterOptions options);

        // Product Variants
        Task<IEnumerable<FBProductVariant>> GetProductVariantsAsync(int productId);
        Task<FBProductVariant> GetVariantByIdAsync(int variantId);

        // Product Images
        Task<IEnumerable<FBProductImage>> GetProductImagesAsync(int productId);
        Task<FBProductImage> GetPrimaryImageAsync(int productId);

        // Product Reviews
        Task<IEnumerable<FBProductReview>> GetProductReviewsAsync(int productId);
        Task<double> GetAverageRatingAsync(int productId);
        Task<bool> AddReviewAsync(FBProductReview review);

        // Categories
        Task<IEnumerable<FBCategory>> GetAllCategoriesAsync();
        Task<IEnumerable<FBCategory>> GetMainCategoriesAsync();
        Task<FBCategory> GetCategoryByIdAsync(int categoryId);

        // Stock & Availability
        Task<bool> IsProductAvailableAsync(int productId, int quantity = 1);
        Task<bool> IsVariantAvailableAsync(int variantId, int quantity = 1);
    }

    /// <summary>
    /// Product Filter Options for Pagination
    /// </summary>
    public class ProductFilterOptions
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int? CategoryId { get; set; }
        public string SearchTerm { get; set; }
        public string SortBy { get; set; } = "newest";
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    /// <summary>
    /// Paged Result for Products
    /// </summary>
    public class ProductPagedResult
    {
        public IEnumerable<FBProduct> Products { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}