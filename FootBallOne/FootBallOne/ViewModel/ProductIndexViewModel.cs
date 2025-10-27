
using FootBallOne.Models;
using System.Collections.Generic;

namespace FootBallOne.ViewModels
{
    /// <summary>
    /// Product Index/Catalog ViewModel
    /// </summary>
    public class ProductIndexViewModel
    {
        public IEnumerable<FBProduct> Products { get; set; }
        public IEnumerable<FBCategory> Categories { get; set; }
        public IEnumerable<string> Brands { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalProducts { get; set; }

        // Filters
        public int? SelectedCategoryId { get; set; }
        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Navigation
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    /// <summary>
    /// Product Details ViewModel
    /// </summary>
    public class ProductDetailsViewModel
    {
        public FBProduct Product { get; set; }
        public IEnumerable<FBProductVariant> Variants { get; set; }
        public IEnumerable<FBProductImage> Images { get; set; }
        public IEnumerable<FBProductReview> Reviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        // Related Products
        public IEnumerable<FBProduct> RelatedProducts { get; set; }

        // Availability
        public bool IsInStock { get; set; }
        public string StockStatus { get; set; }

        // Selected Variant (for add to cart)
        public int? SelectedVariantId { get; set; }
    }

    /// <summary>
    /// Category Products ViewModel
    /// </summary>
    public class CategoryProductsViewModel
    {
        public FBCategory Category { get; set; }
        public IEnumerable<FBCategory> SubCategories { get; set; }
        public IEnumerable<FBProduct> Products { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalProducts { get; set; }

        // Filters
        public string SortBy { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Navigation
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    /// <summary>
    /// Search Results ViewModel
    /// </summary>
    public class SearchResultsViewModel
    {
        public string SearchTerm { get; set; }
        public IEnumerable<FBProduct> Products { get; set; }
        public IEnumerable<FBCategory> Categories { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalProducts { get; set; }

        // Filters
        public int? SelectedCategoryId { get; set; }
        public string SortBy { get; set; }

        // Navigation
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    /// <summary>
    /// Product Review Form ViewModel
    /// </summary>
    public class AddReviewViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Rating { get; set; }
        public string ReviewTitle { get; set; }
        public string ReviewText { get; set; }
    }
}