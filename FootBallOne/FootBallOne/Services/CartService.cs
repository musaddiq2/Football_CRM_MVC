using FootBallOne.Data;
using FootBallOne.Interfaces;
using FootBallOne.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public CartService(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<bool> AddToCartAsync(int userId, int productId, int? variantId, int quantity)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null || !product.IsActive)
                    return false;

                if (variantId.HasValue)
                {
                    var isAvailable = await _productService.IsVariantAvailableAsync(variantId.Value, quantity);
                    if (!isAvailable)
                        return false;
                }
                else
                {
                    var isAvailable = await _productService.IsProductAvailableAsync(productId, quantity);
                    if (!isAvailable)
                        return false;
                }

                // Changed StudentId to Id
                var existingItem = await _context.FBShoppingCartItems
                    .FirstOrDefaultAsync(c => c.Id == userId &&
                                             c.ProductId == productId &&
                                             c.VariantId == variantId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var cartItem = new FBShoppingCartItem
                    {
                        Id = userId,  // Changed from StudentId
                        ProductId = productId,
                        VariantId = variantId,
                        Quantity = quantity,
                        AddedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.FBShoppingCartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                    return await RemoveFromCartAsync(cartItemId);

                var cartItem = await _context.FBShoppingCartItems
                    .Include(c => c.Product)
                    .Include(c => c.Variant)
                    .FirstOrDefaultAsync(c => c.CartItemId == cartItemId);

                if (cartItem == null)
                    return false;

                if (cartItem.VariantId.HasValue)
                {
                    var isAvailable = await _productService.IsVariantAvailableAsync(cartItem.VariantId.Value, quantity);
                    if (!isAvailable)
                        return false;
                }
                else
                {
                    var isAvailable = await _productService.IsProductAvailableAsync(cartItem.ProductId, quantity);
                    if (!isAvailable)
                        return false;
                }

                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            try
            {
                var cartItem = await _context.FBShoppingCartItems.FindAsync(cartItemId);
                if (cartItem == null)
                    return false;

                _context.FBShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            try
            {
                // Changed StudentId to Id
                var cartItems = await _context.FBShoppingCartItems
                    .Where(c => c.Id == userId)
                    .ToListAsync();

                _context.FBShoppingCartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<FBShoppingCartItem>> GetCartItemsAsync(int userId)
        {
            // Changed StudentId to Id
            return await _context.FBShoppingCartItems
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .Include(c => c.Product.ProductImages.Where(i => i.IsPrimary))
                .Include(c => c.Variant)
                .Where(c => c.Id == userId)
                .OrderByDescending(c => c.AddedAt)
                .ToListAsync();
        }

        public async Task<FBShoppingCartItem> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.FBShoppingCartItems
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .FirstOrDefaultAsync(c => c.CartItemId == cartItemId);
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            // Changed StudentId to Id
            return await _context.FBShoppingCartItems
                .Where(c => c.Id == userId)
                .SumAsync(c => c.Quantity);
        }

        public async Task<decimal> GetCartSubtotalAsync(int userId)
        {
            var cartItems = await GetCartItemsAsync(userId);
            return cartItems.Sum(c => c.ItemTotal);
        }

        public async Task<decimal> GetCartTotalAsync(int userId, decimal shippingCost = 0, decimal taxRate = 0)
        {
            var subtotal = await GetCartSubtotalAsync(userId);
            var tax = subtotal * (taxRate / 100);
            return subtotal + shippingCost + tax;
        }

        public async Task<bool> ValidateCartAsync(int userId)
        {
            var errors = await GetCartValidationErrorsAsync(userId);
            return !errors.Any();
        }

        public async Task<List<string>> GetCartValidationErrorsAsync(int userId)
        {
            var errors = new List<string>();
            var cartItems = await GetCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                errors.Add("Your cart is empty.");
                return errors;
            }

            foreach (var item in cartItems)
            {
                if (item.Product == null || !item.Product.IsActive)
                {
                    errors.Add($"{item.Product?.ProductName ?? "Product"} is no longer available.");
                    continue;
                }

                if (item.VariantId.HasValue)
                {
                    var variant = item.Variant;
                    if (variant == null || !variant.IsActive)
                    {
                        errors.Add($"{item.Product.ProductName} variant is no longer available.");
                        continue;
                    }

                    if (variant.StockQuantity < item.Quantity)
                    {
                        errors.Add($"{item.Product.ProductName} - Only {variant.StockQuantity} left in stock.");
                    }
                }
                else
                {
                    if (item.Product.StockQuantity < item.Quantity)
                    {
                        errors.Add($"{item.Product.ProductName} - Only {item.Product.StockQuantity} left in stock.");
                    }
                }
            }

            return errors;
        }

        public async Task<bool> IsProductInCartAsync(int userId, int productId, int? variantId = null)
        {
            // Changed StudentId to Id
            return await _context.FBShoppingCartItems
                .AnyAsync(c => c.Id == userId &&
                              c.ProductId == productId &&
                              c.VariantId == variantId);
        }
    }
}