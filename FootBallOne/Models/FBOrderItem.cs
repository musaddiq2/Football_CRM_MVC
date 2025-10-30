using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    [Table("FBOrderItems", Schema = "WinfoIntern")]
    public class FBOrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? VariantId { get; set; }

        [StringLength(200)]
        public string ProductName { get; set; }

        [StringLength(50)]
        public string SKU { get; set; }

        [StringLength(20)]
        public string Size { get; set; }

        [StringLength(50)]
        public string Color { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public string VariantInfo
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrEmpty(Size)) parts.Add($"Size: {Size}");
                if (!string.IsNullOrEmpty(Color)) parts.Add($"Color: {Color}");
                return parts.Count > 0 ? string.Join(" | ", parts) : "Standard";
            }
        }

        [ForeignKey("OrderId")]
        public virtual FBOrder Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual FBProduct Product { get; set; }

        [ForeignKey("VariantId")]
        public virtual FBProductVariant Variant { get; set; }
    }
}