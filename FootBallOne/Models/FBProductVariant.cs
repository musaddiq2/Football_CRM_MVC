using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    [Table("FBProductVariants", Schema = "WinfoIntern")]
    public class FBProductVariant
    {
        [Key]
        public int VariantId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [StringLength(20)]
        public string Size { get; set; }

        [StringLength(50)]
        public string Color { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalPrice { get; set; } = 0;

        [Required]
        public int StockQuantity { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
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