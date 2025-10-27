using FootBallOne.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallOne.ViewModels
{
    /// <summary>
    /// Checkout Page ViewModel
    /// </summary>
    public class CheckoutViewModel
    {
        // Cart Summary
        public IEnumerable<FBShoppingCartItem> CartItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Total { get; set; }
        public int TotalItems { get; set; }

        // Shipping Information
        [Required(ErrorMessage = "Shipping address is required")]
        [Display(Name = "Address")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string ShippingCity { get; set; }

        [Required(ErrorMessage = "State is required")]
        [Display(Name = "State")]
        public string ShippingState { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [Display(Name = "Postal Code")]
        public string ShippingPostalCode { get; set; }

        [Display(Name = "Country")]
        public string ShippingCountry { get; set; } = "India";

        // Contact Information
        [Required(ErrorMessage = "Contact name is required")]
        [Display(Name = "Full Name")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string ContactEmail { get; set; }

        // Payment Information
        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        // Additional Information
        [Display(Name = "Order Notes (Optional)")]
        public string OrderNotes { get; set; }
    }

    /// <summary>
    /// Order Confirmation ViewModel
    /// </summary>
    public class OrderConfirmationViewModel
    {
        public FBOrder Order { get; set; }
        public IEnumerable<FBOrderItem> OrderItems { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }

    /// <summary>
    /// Order History ViewModel
    /// </summary>
    public class OrderHistoryViewModel
    {
        public IEnumerable<FBOrder> Orders { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    /// <summary>
    /// Order Details ViewModel
    /// </summary>
    public class OrderDetailsViewModel
    {
        public FBOrder Order { get; set; }
        public IEnumerable<FBOrderItem> OrderItems { get; set; }
    }
}