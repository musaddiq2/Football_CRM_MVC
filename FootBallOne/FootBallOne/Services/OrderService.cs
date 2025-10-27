using FootBallOne.Data;
using FootBallOne.Interfaces;
using FootBallOne.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;

        public OrderService(
            ApplicationDbContext context,
            ICartService cartService,
            IConfiguration configuration)
        {
            _context = context;
            _cartService = cartService;
            _configuration = configuration;
        }

        public async Task<FBOrder> CreateOrderAsync(int userId, CheckoutInfo checkoutInfo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                if (!cartItems.Any())
                    throw new Exception("Cart is empty");

                var isValid = await _cartService.ValidateCartAsync(userId);
                if (!isValid)
                    throw new Exception("Cart validation failed");

                var subtotal = await _cartService.GetCartSubtotalAsync(userId);

                var shippingCost = decimal.Parse(_configuration["ECommerce:DefaultShippingCost"] ?? "50");
                var freeShippingThreshold = decimal.Parse(_configuration["ECommerce:FreeShippingThreshold"] ?? "500");
                var taxRate = decimal.Parse(_configuration["ECommerce:TaxRate"] ?? "18");

                var actualShippingCost = subtotal >= freeShippingThreshold ? 0 : shippingCost;
                var taxAmount = subtotal * (taxRate / 100);
                var totalAmount = subtotal + actualShippingCost + taxAmount;

                var orderNumber = await GenerateOrderNumberAsync();

                // Get AcademyId from checkout info if provided, otherwise null
                var academyId = checkoutInfo.AcademyId;

                var order = new FBOrder
                {
                    Id = userId,  // Changed from StudentId to Id
                    AcademyId = academyId,   // ADDED: Store which academy this order belongs to
                    OrderNumber = orderNumber,
                    OrderStatus = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    PaymentMethod = checkoutInfo.PaymentMethod,
                    TransactionId = checkoutInfo.TransactionId,
                    SubTotal = subtotal,
                    DiscountAmount = 0,
                    ShippingCost = actualShippingCost,
                    TaxAmount = taxAmount,
                    TotalAmount = totalAmount,

                    ShippingAddress = checkoutInfo.ShippingAddress,
                    ShippingCity = checkoutInfo.ShippingCity,
                    ShippingState = checkoutInfo.ShippingState,
                    ShippingPostalCode = checkoutInfo.ShippingPostalCode,
                    ShippingCountry = checkoutInfo.ShippingCountry,

                    ContactName = checkoutInfo.ContactName,
                    ContactPhone = checkoutInfo.ContactPhone,
                    ContactEmail = checkoutInfo.ContactEmail,

                    OrderNotes = checkoutInfo.OrderNotes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.FBOrders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var cartItem in cartItems)
                {
                    var unitPrice = cartItem.Product.FinalPrice;
                    if (cartItem.Variant != null)
                    {
                        unitPrice += cartItem.Variant.AdditionalPrice;
                    }

                    var orderItem = new FBOrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = cartItem.ProductId,
                        VariantId = cartItem.VariantId,
                        ProductName = cartItem.Product.ProductName,
                        SKU = cartItem.Variant?.SKU ?? cartItem.Product.SKU,
                        Size = cartItem.Variant?.Size,
                        Color = cartItem.Variant?.Color,
                        Quantity = cartItem.Quantity,
                        UnitPrice = unitPrice,
                        DiscountAmount = 0,
                        TotalPrice = unitPrice * cartItem.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.FBOrderItems.Add(orderItem);

                    if (cartItem.VariantId.HasValue)
                    {
                        var variant = await _context.FBProductVariants.FindAsync(cartItem.VariantId.Value);
                        if (variant != null)
                        {
                            variant.StockQuantity -= cartItem.Quantity;
                        }
                    }
                    else
                    {
                        var product = await _context.FBProducts.FindAsync(cartItem.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity -= cartItem.Quantity;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await _cartService.ClearCartAsync(userId);
                await transaction.CommitAsync();

                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var prefix = _configuration["ECommerce:OrderNumberPrefix"] ?? "ORD";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);

            var orderNumber = $"{prefix}-{timestamp}-{random}";

            while (await _context.FBOrders.AnyAsync(o => o.OrderNumber == orderNumber))
            {
                random = new Random().Next(1000, 9999);
                orderNumber = $"{prefix}-{timestamp}-{random}";
            }

            return orderNumber;
        }

        public async Task<FBOrder> GetOrderByIdAsync(int orderId)
        {
            return await _context.FBOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<FBOrder> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.FBOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<FBOrder>> GetOrdersByStudentAsync(int userId)
        {
            // Changed StudentId to Id
            return await _context.FBOrders
                .Include(o => o.OrderItems)
                .Where(o => o.Id == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBOrder>> GetAllOrdersAsync()
        {
            return await _context.FBOrders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FBOrder>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.FBOrders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var order = await _context.FBOrders.FindAsync(orderId);
                if (order == null)
                    return false;

                order.OrderStatus = status;
                order.UpdatedAt = DateTime.UtcNow;

                if (status == OrderStatus.Shipped)
                {
                    order.ShippedAt = DateTime.UtcNow;
                }
                else if (status == OrderStatus.Delivered)
                {
                    order.DeliveredAt = DateTime.UtcNow;
                }
                else if (status == OrderStatus.Cancelled)
                {
                    order.CancelledAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int orderId, string status)
        {
            try
            {
                var order = await _context.FBOrders.FindAsync(orderId);
                if (order == null)
                    return false;

                order.PaymentStatus = status;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            try
            {
                var order = await _context.FBOrders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    return false;

                if (order.OrderStatus != OrderStatus.Pending &&
                    order.OrderStatus != OrderStatus.Confirmed)
                    return false;

                order.OrderStatus = OrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;
                order.CancellationReason = reason;
                order.UpdatedAt = DateTime.UtcNow;

                foreach (var item in order.OrderItems)
                {
                    if (item.VariantId.HasValue)
                    {
                        var variant = await _context.FBProductVariants.FindAsync(item.VariantId.Value);
                        if (variant != null)
                        {
                            variant.StockQuantity += item.Quantity;
                        }
                    }
                    else
                    {
                        var product = await _context.FBProducts.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<FBOrderItem>> GetOrderItemsAsync(int orderId)
        {
            return await _context.FBOrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.ProductImages.Where(i => i.IsPrimary))
                .Include(oi => oi.Variant)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<int> GetOrderCountByStudentAsync(int userId)
        {
            // Changed StudentId to Id
            return await _context.FBOrders
                .Where(o => o.Id == userId)
                .CountAsync();
        }

        public async Task<decimal> GetTotalSpentByStudentAsync(int userId)
        {
            // Changed StudentId to Id
            return await _context.FBOrders
                .Where(o => o.Id == userId &&
                           o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<OrderStatistics> GetOrderStatisticsAsync()
        {
            var allOrders = await _context.FBOrders.ToListAsync();

            return new OrderStatistics
            {
                TotalOrders = allOrders.Count,
                PendingOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Pending),
                ProcessingOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Processing),
                DeliveredOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Delivered),
                CancelledOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Cancelled),
                TotalRevenue = allOrders.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.TotalAmount),
                AverageOrderValue = allOrders.Any() ? allOrders.Average(o => o.TotalAmount) : 0
            };
        }
    }
}