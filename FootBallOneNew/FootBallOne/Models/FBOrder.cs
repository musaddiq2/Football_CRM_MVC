using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootBallOne.Models
{
    /// <summary>
    /// Order Entity
    /// </summary>
    [Table("FBOrders")]
    public class FBOrder
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Student ID")]
        public int Id { get; set; }

        // CRITICAL: This is the AcademyID that filters orders
        [Display(Name = "Academy ID")]
        public int? AcademyId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Order Status")]
        public string OrderStatus { get; set; } = "Pending";

        [Required]
        [StringLength(50)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [StringLength(100)]
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Shipping Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ShippingCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TaxAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }

        // Shipping Information
        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(500)]
        [Display(Name = "Shipping Address")]
        [DataType(DataType.MultilineText)]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        [Display(Name = "City")]
        public string ShippingCity { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        [Display(Name = "State")]
        public string ShippingState { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string ShippingPostalCode { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string ShippingCountry { get; set; } = "India";

        // Contact Information
        [Required(ErrorMessage = "Contact name is required")]
        [StringLength(200)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Contact phone is required")]
        [StringLength(20)]
        [Display(Name = "Contact Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Contact email is required")]
        [StringLength(256)]
        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string ContactEmail { get; set; }

        // Additional Information
        [Display(Name = "Order Notes")]
        [DataType(DataType.MultilineText)]
        public string OrderNotes { get; set; }

        [StringLength(100)]
        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }

        [Display(Name = "Shipped At")]
        public DateTime? ShippedAt { get; set; }

        [Display(Name = "Delivered At")]
        public DateTime? DeliveredAt { get; set; }

        [Display(Name = "Cancelled At")]
        public DateTime? CancelledAt { get; set; }

        [StringLength(500)]
        [Display(Name = "Cancellation Reason")]
        public string CancellationReason { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        public bool IsPending => OrderStatus == "Pending";

        [NotMapped]
        public bool IsConfirmed => OrderStatus == "Confirmed";

        [NotMapped]
        public bool IsShipped => OrderStatus == "Shipped";

        [NotMapped]
        public bool IsDelivered => OrderStatus == "Delivered";

        [NotMapped]
        public bool IsCancelled => OrderStatus == "Cancelled";

        [NotMapped]
        public bool IsRefunded => OrderStatus == "Refunded";

        [NotMapped]
        [Display(Name = "Full Shipping Address")]
        public string FullShippingAddress =>
            $"{ShippingAddress}, {ShippingCity}, {ShippingState} {ShippingPostalCode}, {ShippingCountry}";

        // Navigation Properties
        // REMOVED: [ForeignKey("AcademyId")] - causing issues
        // Just keep the int? AcademyId property, no navigation property to Academy needed

        public virtual ICollection<FBOrderItem> OrderItems { get; set; }

        public FBOrder()
        {
            OrderItems = new HashSet<FBOrderItem>();
        }
    }

    /// <summary>
    /// Order Status Enum
    /// </summary>
    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
        public const string Refunded = "Refunded";
    }

    /// <summary>
    /// Payment Status Enum
    /// </summary>
    public static class PaymentStatus
    {
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Failed = "Failed";
        public const string Refunded = "Refunded";
    }
}