using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    /// <summary>
    /// Product Variant Entity (Size, Color variations)
    /// </summary>
    [Table("FBProductVariants")]
    public class FBProductVariant
    {
        [Key]
        public int VariantId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [StringLength(20)]
        [Display(Name = "Size")]
        public string Size { get; set; }

        [StringLength(50)]
        [Display(Name = "Color")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Variant SKU is required")]
        [StringLength(50)]
        [Display(Name = "Variant SKU")]
        public string SKU { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        [Display(Name = "Additional Price")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal AdditionalPrice { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        [Display(Name = "Variant Display")]
        public string VariantDisplay
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(Size)) parts.Add($"Size: {Size}");
                if (!string.IsNullOrEmpty(Color)) parts.Add($"Color: {Color}");
                return string.Join(" | ", parts);
            }
        }

        [NotMapped]
        public bool IsOutOfStock => StockQuantity == 0;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual FBProduct Product { get; set; }

        public virtual ICollection<FBShoppingCartItem> ShoppingCartItems { get; set; }
        public virtual ICollection<FBOrderItem> OrderItems { get; set; }

        public FBProductVariant()
        {
            ShoppingCartItems = new HashSet<FBShoppingCartItem>();
            OrderItems = new HashSet<FBOrderItem>();
        }
    }
}