using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    [Table("FBShoppingCartItems", Schema = "WinfoIntern")]
    public class FBShoppingCartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? VariantId { get; set; }

        [Required]
        [Range(1, 999)]
        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal ItemTotal
        {
            get
            {
                if (Product == null) return 0;
                var basePrice = Product.FinalPrice;
                var additionalPrice = Variant?.AdditionalPrice ?? 0;
                return (basePrice + additionalPrice) * Quantity;
            }
        }

        [ForeignKey("ProductId")]
        public virtual FBProduct Product { get; set; }

        [ForeignKey("VariantId")]
        public virtual FBProductVariant Variant { get; set; }
    }
}