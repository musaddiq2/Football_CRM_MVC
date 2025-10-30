using Microsoft.AspNetCore.Mvc;
using FootBallOne.ViewModel;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;
using FootBallOne.Models;
using FootBallOne.Services;
using static FootBallOne.ViewModel.DashboardViewModelnew;

namespace FootBallOne.Controllers
{
    public class AdminDashboard : BaseController
    {
        private readonly ApplicationDbContext _context;
        public AdminDashboard(ApplicationDbContext context, IAcademyContext academyContext)
            : base(academyContext)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var registrationsQuery = _context.RGManagements.AsQueryable();
            if (!IsSuperAdmin && CurrentAcademyId.HasValue)
            {
                registrationsQuery = registrationsQuery.Where(r => r.AcademyID == CurrentAcademyId);
            }

            var activeSubs = registrationsQuery
                .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
                .ToList();

            // Current and last month ranges
            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var endOfThisMonth = startOfThisMonth.AddMonths(1).AddDays(-1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);
            var endOfLastMonth = startOfThisMonth.AddDays(-1);

            // New subscriptions this month
            var newThisMonth = registrationsQuery
                .Where(s => s.CreatedDate.HasValue && s.CreatedDate.Value >= startOfThisMonth && s.CreatedDate.Value <= endOfThisMonth)
                .Count();
            var newLastMonth = registrationsQuery
                .Where(s => s.CreatedDate.HasValue && s.CreatedDate.Value >= startOfLastMonth && s.CreatedDate.Value <= endOfLastMonth)
                .Count();

            // Percentage change
            double percentageChange = newLastMonth > 0
                ? ((double)(newThisMonth - newLastMonth) / newLastMonth) * 100
                : 0;

            // Suspended subscriptions
            var suspendedCount = registrationsQuery
                .Count(s => s.SubscriptionEnd < DateTime.Now);

            // Expiring soon
            var today = DateTime.Now;
            var sevenDaysFromNow = today.AddDays(7);
            var expiringSoonCount = registrationsQuery
                .Count(s => s.SubscriptionEnd.HasValue && s.SubscriptionEnd >= today && s.SubscriptionEnd <= sevenDaysFromNow && s.PaymentStatus == "Paid");

            // Current month income and expenses
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var currentMonthIncome = registrationsQuery
                .Where(s => s.SubscriptionStart.HasValue && s.SubscriptionStart.Value >= startOfMonth && s.SubscriptionStart.Value <= endOfMonth)
                .Sum(s => s.SubscriptionFees)
                .GetValueOrDefault();
            var currentMonthExpenses = registrationsQuery
                .Where(s => s.SubscriptionStart.HasValue && s.SubscriptionStart.Value >= startOfMonth && s.SubscriptionStart.Value <= endOfMonth)
                .Sum(s => s.KitFees + s.SugarFees + s.BagFees)
                .GetValueOrDefault();

            // Active courses
            var activeCourses = registrationsQuery
                .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
                .Select(s => s.Category)
                .Distinct()
                .Count();
            var activeCourseList = registrationsQuery
                .Where(s => s.SubscriptionEnd >= DateTime.Now && s.PaymentStatus == "Paid")
                .Select(s => s.Category)
                .Distinct()
                .ToList();

            // Yearly totals
            var yearlyIncome = registrationsQuery
                .Where(s => s.SubscriptionStart.HasValue && s.SubscriptionStart.Value.Year == now.Year)
                .Sum(s => s.SubscriptionFees)
                .GetValueOrDefault();
            var yearlyExpenses = registrationsQuery
                .Where(s => s.SubscriptionStart.HasValue && s.SubscriptionStart.Value.Year == now.Year)
                .Sum(s => s.KitFees + s.SugarFees + s.BagFees)
                .GetValueOrDefault();

            // Monthly financial data
            var monthlyData = Enumerable.Range(0, 12).Select(i =>
            {
                var monthStart = new DateTime(now.Year, i + 1, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var income = registrationsQuery
                    .Where(s => s.SubscriptionStart.HasValue && s.SubscriptionStart.Value >= monthStart && s.SubscriptionStart.Value <= monthEnd)
                    .Sum(s => s.SubscriptionFees)
                    .GetValueOrDefault();
                var expenses = registrationsQuery
                    .Where(s => s.SubscriptionStart.HasValue && s.SubscriptionStart.Value >= monthStart && s.SubscriptionStart.Value <= monthEnd)
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

            // Yearly financial summary
            var yearlySummary = new Dictionary<string, List<decimal>>();
            foreach (var year in new[] { now.Year - 1, now.Year })
            {
                var incomeQuarters = new List<decimal>();
                var expenseQuarters = new List<decimal>();
                for (int q = 1; q <= 4; q++)
                {
                    var startMonth = (q - 1) * 3 + 1;
                    var startDate = new DateTime(year, startMonth, 1);
                    var endDate = startDate.AddMonths(3).AddDays(-1);
                    var income = registrationsQuery
                        .Where(s => s.SubscriptionStart.HasValue && s.SubscriptionStart.Value >= startDate && s.SubscriptionStart.Value <= endDate)
                        .Sum(s => s.SubscriptionFees)
                        .GetValueOrDefault();
                    var expenses = registrationsQuery
                        .Where(s => s.SubscriptionStart.HasValue && s.SubscriptionStart.Value >= startDate && s.SubscriptionStart.Value <= endDate)
                        .Sum(s => s.KitFees + s.SugarFees + s.BagFees)
                        .GetValueOrDefault();
                    incomeQuarters.Add(income);
                    expenseQuarters.Add(expenses);
                }
                yearlySummary[$"{year}_Income"] = incomeQuarters;
                yearlySummary[$"{year}_Expenses"] = expenseQuarters;
            }

            // Financial distributions
            var incomeDistribution = registrationsQuery
                .Where(s => s.PaymentStatus == "Paid")
                .GroupBy(s => s.Category ?? "Unknown")
                .Select(g => new DashboardViewModelnew.FinancialSource
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(s => s.SubscriptionFees) ?? 0
                })
                .OrderByDescending(g => g.TotalAmount)
                .ToList();

            var expenseDistribution = registrationsQuery
                .Where(s => s.PaymentStatus == "Paid")
                .GroupBy(s => s.Category ?? "Unknown")
                .Select(g => new DashboardViewModelnew.FinancialSource
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(s => s.KitFees + s.SugarFees + s.BagFees) ?? 0
                })
                .OrderByDescending(g => g.TotalAmount)
                .ToList();

            var netProfitDistribution = registrationsQuery
                .Where(s => s.PaymentStatus == "Paid")
                .GroupBy(s => s.Category ?? "Unknown")
                .Select(g => new DashboardViewModelnew.FinancialSource
                {
                    Category = g.Key,
                    TotalAmount = (g.Sum(s => s.SubscriptionFees) ?? 0) - (g.Sum(s => s.KitFees + s.SugarFees + s.BagFees) ?? 0)
                })
                .OrderByDescending(g => g.TotalAmount)
                .ToList();

            var model = new DashboardViewModelnew
            {
                ActiveSubscriptions = activeSubs.Count,
                ActiveSubscriptionsList = activeSubs,
                ActiveSubscribers = activeSubs.Select(s => s.AcademyID).Distinct().Count(), // Adjust if UserId is not available
                NewThisMonth = newThisMonth,
                NewSubscriptionChangePercentage = Math.Round((decimal)percentageChange, 1),
                Suspended = suspendedCount,
                ExpiringSoon = expiringSoonCount,
                CurrentMonthIncome = currentMonthIncome,
                CurrentMonthExpenses = currentMonthExpenses,
                YearlyIncome = yearlyIncome,
                YearlyExpenses = yearlyExpenses,
                ProfitChangePercentage = Math.Round((decimal)percentageChange, 1), // Adjust if different metric needed
                MonthlyExpenses = monthlyData.Select(m => m.Expenses).ToList(),
                MonthlyProfit = monthlyData.Select(m => m.Profit).ToList(),
                ActiveCourses = activeCourses,
                ActiveCourseList = activeCourseList,
                MonthlyFinancialData = monthlyData,
                YearlyFinancialSummary = yearlySummary,
                IncomeDistribution = incomeDistribution,
                ExpenseDistribution = expenseDistribution,
                NetProfitDistribution = netProfitDistribution
            };

            return View(model);
        }
    }
}