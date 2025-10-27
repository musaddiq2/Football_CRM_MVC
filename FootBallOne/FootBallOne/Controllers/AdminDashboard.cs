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
            // 1. Retrieve the AcademyID from the session
            // The value is an int?, use GetInt32.
            int? academyId = HttpContext.Session.GetInt32("AcademyID");

            // Optional: If you have a Super Admin who has AcademyID = 0 (or null in the DB, but 0 in session)
            // You can implement logic here to grant full access if academyId == 0.
            // For now, we will assume 0 means NO filter (Super Admin) or it's just filtered by AcademyID > 0.

            // 2. Define the base query, which is an IQueryable (not yet executed)
            var baseQuery = _context.RGManagements.AsQueryable();

            if (academyId.HasValue && academyId.Value > 0)
            {
                // Apply the AcademyId filter for the Academy Admin
                // Assuming RGManagement (RGManagements table) has an AcademyID property
                baseQuery = baseQuery.Where(s => s.AcademyID == academyId.Value);
            }
            // If academyId is null or 0, the baseQuery remains unfiltered (Super Admin view).

            // --------------------------------------------------------------------------------
            // All subsequent queries use 'baseQuery' instead of '_context.RGManagements'
            // --------------------------------------------------------------------------------

            // Active Subscriptions
            var activeSubs = baseQuery
                .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
                .ToList();

            // Date Ranges for New Subscriptions
            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var endOfThisMonth = startOfThisMonth.AddMonths(1).AddDays(-1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);
            var endOfLastMonth = startOfThisMonth.AddDays(-1);

            // New Subscriptions (This Month vs. Last Month)
            var newThisMonth = baseQuery
                .Count(s => s.CreatedDate >= startOfThisMonth && s.CreatedDate <= endOfThisMonth);

            var newLastMonth = baseQuery
                .Count(s => s.CreatedDate >= startOfLastMonth && s.CreatedDate <= endOfLastMonth);

            // Calculate percentage change
            double percentageChange = 0;
            if (newLastMonth > 0)
            {
                percentageChange = ((double)(newThisMonth - newLastMonth) / newLastMonth) * 100;
            }

            // Suspended Subscriptions
            var suspendedCount = baseQuery
                .Count(s => s.SubscriptionEnd < DateTime.Now);

            // Expiring Soon
            var today = DateTime.Now;
            var sevenDaysFromNow = today.AddDays(7);

            var expiringSoonCount = baseQuery
                .Count(s => s.SubscriptionEnd >= today && s.SubscriptionEnd <= sevenDaysFromNow);

            // Current Month Financials (Income/Expenses)
            var nowForIncome = DateTime.Now;
            var startOfMonth = new DateTime(nowForIncome.Year, nowForIncome.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var currentMonthIncome = baseQuery
                .Where(s => s.SubscriptionStart >= startOfMonth && s.SubscriptionStart <= endOfMonth)
                .Sum(s => s.SubscriptionFees)
                .GetValueOrDefault();

            var currentMonthExpenses = baseQuery
                .Where(s => s.SubscriptionStart >= startOfMonth && s.SubscriptionStart <= endOfMonth)
                .Sum(s => s.KitFees + s.SugarFees + s.BagFees)
                .GetValueOrDefault();

            // Active Courses Count and List
            var activeCourses = baseQuery
                .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
                .Select(s => s.Category)
                .Distinct()
                .Count();

            var activeCourseList = baseQuery
                .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
                .Select(s => s.Category)
                .Distinct()
                .ToList();

            // Monthly Financial Data (for the current year)
            var monthlyData = Enumerable.Range(0, 12).Select(i =>
            {
                var monthStart = new DateTime(DateTime.Now.Year, i + 1, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var income = baseQuery
                    .Where(s => s.SubscriptionStart >= monthStart && s.SubscriptionStart <= monthEnd)
                    .Sum(s => s.SubscriptionFees)
                    .GetValueOrDefault();

                var expenses = baseQuery
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

            // Yearly Summary (Quarterly Data) - Only use current and previous year for charting
            var yearlySummary = new Dictionary<string, List<decimal>>();

            foreach (var year in new[] { DateTime.Now.Year - 1, DateTime.Now.Year })
            {
                var incomeQuarters = new List<decimal>();
                var expenseQuarters = new List<decimal>();

                for (int q = 1; q <= 4; q++)
                {
                    var startMonth = (q - 1) * 3 + 1;
                    var startDate = new DateTime(year, startMonth, 1);
                    var endDate = startDate.AddMonths(3).AddDays(-1);

                    var income = baseQuery
                        .Where(s => s.SubscriptionStart >= startDate && s.SubscriptionStart <= endDate)
                        .Sum(s => s.SubscriptionFees) ?? 0;

                    var expenses = baseQuery
                        .Where(s => s.SubscriptionStart >= startDate && s.SubscriptionStart <= endDate)
                        .Sum(s => s.KitFees + s.SugarFees + s.BagFees) ?? 0;

                    incomeQuarters.Add(income);
                    expenseQuarters.Add(expenses);
                }

                yearlySummary[$"{year}_Income"] = incomeQuarters;
                yearlySummary[$"{year}_Expenses"] = expenseQuarters;
            }

            // Income Distribution by Category
            var incomeDistribution = baseQuery
                .Where(s => s.PaymentStatus == "Paid")
                .GroupBy(s => s.Category)
                .Select(g => new IncomeSource
                {
                    Category = g.Key,
                    TotalIncome = g.Sum(s => s.SubscriptionFees) ?? 0
                })
                .OrderByDescending(g => g.TotalIncome)
                .ToList();


            // Final ViewModel mapping
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