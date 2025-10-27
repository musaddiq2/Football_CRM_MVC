using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    /// <summary>
    /// Order Item Entity
    /// </summary>
    [Table("FBOrderItems")]
    public class FBOrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required]
        [Display(Name = "Order")]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Variant")]
        public int? VariantId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } // Snapshot at time of order

        [Required]
        [StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; } // Snapshot at time of order

        [StringLength(20)]
        [Display(Name = "Size")]
        public string Size { get; set; }

        [StringLength(50)]
        [Display(Name = "Color")]
        public string Color { get; set; }

        [Required]
        [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal UnitPrice { get; set; } // Price at time of order

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Computed Properties
        [NotMapped]
        [Display(Name = "Variant Info")]
        public string VariantInfo
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrEmpty(Size)) parts.Add($"Size: {Size}");
                if (!string.IsNullOrEmpty(Color)) parts.Add($"Color: {Color}");
                return parts.Count > 0 ? string.Join(" | ", parts) : "No variant";
            }
        }

        // Navigation Properties
        [ForeignKey("OrderId")]
        public virtual FBOrder Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual FBProduct Product { get; set; }

        [ForeignKey("VariantId")]
        public virtual FBProductVariant Variant { get; set; }
    }
}