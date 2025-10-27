using FootBallOne.Models;
using System.Collections.Generic;

namespace FootBallOne.ViewModels
{
    /// <summary>
    /// Shopping Cart ViewModel
    /// </summary>
    public class CartViewModel
    {
        public IEnumerable<FBShoppingCartItem> CartItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Total { get; set; }
        public int TotalItems { get; set; }
        public List<string> ValidationErrors { get; set; }
        public bool IsValid { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public bool QualifiesForFreeShipping => Subtotal >= FreeShippingThreshold;
        public decimal AmountNeededForFreeShipping => FreeShippingThreshold - Subtotal;
    }

    /// <summary>
    /// Add to Cart Request Model
    /// </summary>
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// Update Cart Item Request Model
    /// </summary>
    public class UpdateCartRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}