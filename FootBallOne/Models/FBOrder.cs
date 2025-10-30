using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    [Table("FBOrders", Schema = "WinfoIntern")]
    public class FBOrder
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int Id { get; set; }

        public int? AcademyId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        [StringLength(50)]
        public string OrderStatus { get; set; } = "Pending";

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(100)]
        public string TransactionId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string ShippingCity { get; set; }

        [StringLength(100)]
        public string ShippingState { get; set; }

        [StringLength(20)]
        public string ShippingPostalCode { get; set; }

        [StringLength(100)]
        public string ShippingCountry { get; set; } = "India";

        [StringLength(200)]
        public string ContactName { get; set; }

        [StringLength(20)]
        public string ContactPhone { get; set; }

        [StringLength(256)]
        public string ContactEmail { get; set; }

        public string OrderNotes { get; set; }

        [StringLength(100)]
        public string TrackingNumber { get; set; }

        public DateTime? ShippedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        [StringLength(500)]
        public string CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<FBOrderItem> OrderItems { get; set; }

        public FBOrder()
        {
            OrderItems = new HashSet<FBOrderItem>();
        }
    }
}