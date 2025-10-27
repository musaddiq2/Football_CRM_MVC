using FootBallOne.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootBallOne.Interfaces
{
    /// <summary>
    /// Shopping Cart Service Interface
    /// </summary>
    public interface ICartService
    {
        // Cart Operations
        Task<bool> AddToCartAsync(int studentId, int productId, int? variantId, int quantity);
        Task<bool> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(int studentId);

        // Cart Retrieval
        Task<IEnumerable<FBShoppingCartItem>> GetCartItemsAsync(int studentId);
        Task<FBShoppingCartItem> GetCartItemByIdAsync(int cartItemId);
        Task<int> GetCartItemCountAsync(int studentId);

        // Cart Calculations
        Task<decimal> GetCartSubtotalAsync(int studentId);
        Task<decimal> GetCartTotalAsync(int studentId, decimal shippingCost = 0, decimal taxRate = 0);

        // Cart Validation
        Task<bool> ValidateCartAsync(int studentId);
        Task<List<string>> GetCartValidationErrorsAsync(int studentId);
        Task<bool> IsProductInCartAsync(int studentId, int productId, int? variantId = null);
    }
}