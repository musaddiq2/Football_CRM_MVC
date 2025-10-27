using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    /// <summary>
    /// Shopping Cart Item Entity
    /// </summary>
    [Table("FBShoppingCartItems")]
    public class FBShoppingCartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        [Display(Name = "Student ID")]
        public int Id { get; set; } // References your existing Student table

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Variant")]
        public int? VariantId { get; set; }

        [Required]
        [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Added At")]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        [Display(Name = "Item Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
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

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual FBProduct Product { get; set; }

        [ForeignKey("VariantId")]
        public virtual FBProductVariant Variant { get; set; }

        // Note: No navigation to Student - managed in your existing system
    }
}