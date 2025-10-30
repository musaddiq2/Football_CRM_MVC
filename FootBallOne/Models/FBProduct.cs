using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    [Table("FBProducts", Schema = "WinfoIntern")]
    public class FBProduct
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; }

        public string Description { get; set; }

        [StringLength(100)]
        public string Brand { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        [Required]
        public int StockQuantity { get; set; } = 0;

        [Required]
        public int MinStockLevel { get; set; } = 10;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Weight { get; set; }

        [StringLength(100)]
        public string Dimensions { get; set; }

        [StringLength(500)]
        public string Tags { get; set; }

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal FinalPrice => BasePrice - (BasePrice * DiscountPercentage / 100);

        [NotMapped]
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= MinStockLevel;

        [NotMapped]
        public bool IsOutOfStock => StockQuantity == 0;

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