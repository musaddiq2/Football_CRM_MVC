using FootBallOne.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootBallOne.Interfaces
{
    public interface IOrderService
    {
        // Order Creation
        Task<FBOrder> CreateOrderAsync(int userId, CheckoutInfo checkoutInfo);
        Task<string> GenerateOrderNumberAsync();

        // Order Retrieval
        Task<FBOrder> GetOrderByIdAsync(int orderId);
        Task<FBOrder> GetOrderByNumberAsync(string orderNumber);
        Task<IEnumerable<FBOrder>> GetOrdersByStudentAsync(int userId);
        Task<IEnumerable<FBOrder>> GetAllOrdersAsync();
        Task<IEnumerable<FBOrder>> GetRecentOrdersAsync(int count = 10);

        // Order Status Management
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> UpdatePaymentStatusAsync(int orderId, string status);
        Task<bool> CancelOrderAsync(int orderId, string reason);

        // Order Items
        Task<IEnumerable<FBOrderItem>> GetOrderItemsAsync(int orderId);

        // Order Statistics
        Task<int> GetOrderCountByStudentAsync(int userId);
        Task<decimal> GetTotalSpentByStudentAsync(int userId);
        Task<OrderStatistics> GetOrderStatisticsAsync();
    }

    public class CheckoutInfo
    {
        // Shipping Address
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; } = "India";

        // Contact Information
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        // Payment Information
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }

        // Additional
        public string OrderNotes { get; set; }

        // ADDED: Academy tracking
        public int? AcademyId { get; set; }
    }

    public class OrderStatistics
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}   