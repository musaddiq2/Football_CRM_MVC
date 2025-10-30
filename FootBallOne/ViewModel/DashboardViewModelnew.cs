using FootBallOne.Models;
using System.Collections.Generic;

namespace FootBallOne.ViewModel
{
    public class DashboardViewModelnew
    {
        public int ActiveSubscriptions { get; set; }
        public int ActiveCourses { get; set; }
        public int ActiveSubscribers { get; set; }
        public int NewThisMonth { get; set; }
        public int Suspended { get; set; }
        public int ExpiringSoon { get; set; }
        public decimal CurrentMonthIncome { get; set; }
        public decimal CurrentMonthExpenses { get; set; }
        public decimal CurrentMonthNetProfit => CurrentMonthIncome - CurrentMonthExpenses;
        public decimal YearlyIncome { get; set; }
        public decimal YearlyExpenses { get; set; }
        public decimal ProfitChangePercentage { get; set; }
        public List<decimal> MonthlyExpenses { get; set; }
        public List<decimal> MonthlyProfit { get; set; }
        public List<RegistrationManagement> ActiveSubscriptionsList { get; set; }
        public decimal NewSubscriptionChangePercentage { get; set; }
        public List<string> ActiveCourseList { get; set; }

        public class MonthlyFinance
        {
            public string Month { get; set; }
            public decimal Income { get; set; }
            public decimal Expenses { get; set; }
            public decimal Profit { get; set; }
        }

        public List<MonthlyFinance> MonthlyFinancialData { get; set; }
        public Dictionary<string, List<decimal>> YearlyFinancialSummary { get; set; }

        public class FinancialSource
        {
            public string Category { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public List<FinancialSource> IncomeDistribution { get; set; }
        public List<FinancialSource> ExpenseDistribution { get; set; }
        public List<FinancialSource> NetProfitDistribution { get; set; }
    }
}