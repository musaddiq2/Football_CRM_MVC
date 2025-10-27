using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    /// <summary>
    /// Product Entity
    /// </summary>
    [Table("FBProducts")]
    public class FBProduct
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Brand")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Base price is required")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        [Display(Name = "Base Price")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal BasePrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        [Display(Name = "Discount %")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; } = false;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Min Stock Level")]
        public int MinStockLevel { get; set; } = 10;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Weight (kg)")]
        public decimal? Weight { get; set; }

        [StringLength(100)]
        [Display(Name = "Dimensions (LxWxH)")]
        public string Dimensions { get; set; }

        [StringLength(500)]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        [Display(Name = "View Count")]
        public int ViewCount { get; set; } = 0;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        [Display(Name = "Final Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal FinalPrice => BasePrice - (BasePrice * DiscountPercentage / 100);

        [NotMapped]
        [Display(Name = "Stock Status")]
        public string StockStatus
        {
            get
            {
                if (StockQuantity == 0) return "Out of Stock";
                if (StockQuantity <= MinStockLevel) return "Low Stock";
                return "In Stock";
            }
        }

        [NotMapped]
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= MinStockLevel;

        [NotMapped]
        public bool IsOutOfStock => StockQuantity == 0;

        // Navigation Properties
        [ForeignKey("CategoryId")]
        public virtual FBCategory Category { get; set; }

        public virtual ICollection<FBProductVariant> ProductVariants { get; set; }
        public virtual ICollection<FBProductImage> ProductImages { get; set; }
        public virtual ICollection<FBProductReview> ProductReviews { get; set; }
        public virtual ICollection<FBShoppingCartItem> ShoppingCartItems { get; set; }
        public virtual ICollection<FBOrderItem> OrderItems { get; set; }

        public FBProduct()
        {
            ProductVariants = new HashSet<FBProductVariant>();
            ProductImages = new HashSet<FBProductImage>();
            ProductReviews = new HashSet<FBProductReview>();
            ShoppingCartItems = new HashSet<FBShoppingCartItem>();
            OrderItems = new HashSet<FBOrderItem>();
        }
    }
}