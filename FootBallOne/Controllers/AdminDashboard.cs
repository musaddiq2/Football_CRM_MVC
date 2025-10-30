using Microsoft.AspNetCore.Mvc;
using FootBallOne.ViewModel;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;
using FootBallOne.Models;
using static FootBallOne.ViewModel.DashboardViewModelnew;

namespace FootBallOne.Controllers
{
    public class AdminDashboard : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboard(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            //var model = new DashboardViewModelnew
            //{
            //    //ActiveSubscriptions = 0,
            //    ActiveCourses = 1,
            //    ActiveSubscribers = 0,
            //    NewThisMonth = 0,
            //    Suspended = 0,
            //    ExpiringSoon = 0,
            //    CurrentMonthIncome = 0.00m,
            //    CurrentMonthExpenses = 0.00m,
            //    YearlyIncome = 0.00m,
            //    YearlyExpenses = 0.00m,
            //    ProfitChangePercentage = 0.0m,

            //        MonthlyIncome = new List<decimal> { 0, 0, 1, 1, 2, 2 },
            //    MonthlyExpenses = new List<decimal> { 0, 0, 0.5m, 0.5m, 1, 1 },
            //    MonthlyProfit = new List<decimal> { 0, 0, 0.5m, 1, 1.5m, 1.8m }
            //};

            var activeSubs = _context.RGManagements
        .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
        .ToList();//

            //for activ this month

            var now = DateTime.Now;

            // Current month range
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var endOfThisMonth = startOfThisMonth.AddMonths(1).AddDays(-1);

            // Last month range
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);
            var endOfLastMonth = startOfThisMonth.AddDays(-1);

            // Count new subscriptions
            var newThisMonth = _context.RGManagements
                .Count(s => s.CreatedDate >= startOfThisMonth && s.CreatedDate <= endOfThisMonth);

            var newLastMonth = _context.RGManagements
                .Count(s => s.CreatedDate >= startOfLastMonth && s.CreatedDate <= endOfLastMonth);

            // Calculate percentage change
            double percentageChange = 0;
            if (newLastMonth > 0)
            {
                percentageChange = ((double)(newThisMonth - newLastMonth) / newLastMonth) * 100;//
            }


            // this is  for Suspended means subscription end
            var suspendedCount = _context.RGManagements
    .Count(s => s.SubscriptionEnd < DateTime.Now);//


            // This is for Expiring soon
            var today = DateTime.Now;
            var sevenDaysFromNow = today.AddDays(7);

            var expiringSoonCount = _context.RGManagements
                .Count(s => s.SubscriptionEnd >= today && s.SubscriptionEnd <= sevenDaysFromNow);
            //var today = DateTime.Today;
            //var sevenDaysFromNow = today.AddDays(7);

            //var expiringSoonCount = _context.RGManagements
            //    .Count(s => s.SubscriptionEnd.Date >= today &&
            //                s.SubscriptionEnd.Date <= sevenDaysFromNow &&
            //                s.PaymentStatus == "Paid");

            //

            //This is for current month income
            var nowForIncome = DateTime.Now;
            var startOfMonth = new DateTime(nowForIncome.Year, nowForIncome.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var currentMonthIncome = _context.RGManagements
                .Where(s => s.SubscriptionStart >= startOfMonth && s.SubscriptionStart <= endOfMonth)
                .Sum(s => s.SubscriptionFees)
                .GetValueOrDefault();//


            // Current Month Expenses
            var currentMonthExpenses = _context.RGManagements
    .Where(s => s.SubscriptionStart >= startOfMonth && s.SubscriptionStart <= endOfMonth)
    .Sum(s => s.KitFees + s.SugarFees + s.BagFees)
    .GetValueOrDefault();
            //

            var activeCourses = _context.RGManagements
    .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
    .Select(s => s.Category) // or use CourseName if you have it
    .Distinct()
    .Count();

            var activeCourseList = _context.RGManagements
    .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
    .Select(s => s.Category)
    .Distinct()
    .ToList();
            var currentMonthNetProfit = currentMonthIncome - currentMonthExpenses;



            var monthlyData = Enumerable.Range(0, 12).Select(i =>
            {
                var monthStart = new DateTime(DateTime.Now.Year, i + 1, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var income = _context.RGManagements
                    .Where(s => s.SubscriptionStart >= monthStart && s.SubscriptionStart <= monthEnd)
                    .Sum(s => s.SubscriptionFees)
                    .GetValueOrDefault();

                var expenses = _context.RGManagements
                    .Where(s => s.SubscriptionStart >= monthStart && s.SubscriptionStart <= monthEnd)
                    .Sum(s => s.KitFees + s.SugarFees + s.BagFees)
                    .GetValueOrDefault();

                return new MonthlyFinance
                {
                    Month = monthStart.ToString("MMMM"),
                    Income = income,
                    Expenses = expenses,
                    Profit = income - expenses
                };
            }).ToList();

            //
            var yearlySummary = new Dictionary<string, List<decimal>>();

            foreach (var year in new[] { 2024, 2025 })
            {
                var incomeQuarters = new List<decimal>();
                var expenseQuarters = new List<decimal>();

                for (int q = 1; q <= 4; q++)
                {
                    var startMonth = (q - 1) * 3 + 1;
                    var startDate = new DateTime(year, startMonth, 1);
                    var endDate = startDate.AddMonths(3).AddDays(-1);

                    var income = _context.RGManagements
                        .Where(s => s.SubscriptionStart >= startDate && s.SubscriptionStart <= endDate)
                        .Sum(s => s.SubscriptionFees) ?? 0;

                    var expenses = _context.RGManagements
                        .Where(s => s.SubscriptionStart >= startDate && s.SubscriptionStart <= endDate)
                        .Sum(s => s.KitFees + s.SugarFees + s.BagFees) ?? 0;

                    incomeQuarters.Add(income);
                    expenseQuarters.Add(expenses);
                }

                yearlySummary[$"{year}_Income"] = incomeQuarters;
                yearlySummary[$"{year}_Expenses"] = expenseQuarters;
            }



            //

            var incomeDistribution = _context.RGManagements
                .Where(s => s.PaymentStatus == "Paid")
                .GroupBy(s => s.Category)
                .Select(g => new IncomeSource
                {
                    Category = g.Key,
                    TotalIncome = g.Sum(s => s.SubscriptionFees) ?? 0
                })
                .OrderByDescending(g => g.TotalIncome)
                .ToList();






            var model = new DashboardViewModelnew
            {
                ActiveSubscriptions = activeSubs.Count,
                ActiveSubscriptionsList = activeSubs,
                NewThisMonth = newThisMonth,
                ProfitChangePercentage = Math.Round((decimal)percentageChange, 1),
                Suspended = suspendedCount,
                ExpiringSoon = expiringSoonCount,
                CurrentMonthIncome = currentMonthIncome,
                CurrentMonthExpenses = currentMonthExpenses,
                ActiveCourses = activeCourses,
                ActiveCourseList = activeCourseList,
                MonthlyFinancialData = monthlyData,
                YearlyFinancialSummary = yearlySummary,
                IncomeDistribution = incomeDistribution,



            };




            return View(model);
        }
    }
}