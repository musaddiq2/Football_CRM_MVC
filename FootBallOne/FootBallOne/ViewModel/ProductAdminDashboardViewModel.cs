using FootBallOne.Interfaces;
using FootBallOne.Models;
using System.Collections.Generic;

namespace FootBallOne.ViewModels
{
    /// <summary>
    /// ProductAdmin Dashboard ViewModel
    /// </summary>
    public class ProductAdminDashboardViewModel
    {
        public OrderStatistics OrderStatistics { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int? AcademyID { get; set; }
        public IEnumerable<FBOrder> RecentOrders { get; set; }
    }
}