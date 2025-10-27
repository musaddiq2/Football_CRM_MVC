namespace FootBallOne.ViewModel
{
    public class DashboardViewModel
    {
        public int TotalPlayers { get; set; }
        public int TotalPaidPlayers { get; set; }
        public int TotalUnpaidPlayers { get; set; }
        public int TotalCoaches { get; set; }
        public int NewPlayerRequests { get; set; }
        public int ExpiringSubscriptions { get; set; }
        public int TotalRevenue { get; set; }

        public List<string> MonthlyLabels { get; set; }
        public List<int> MonthlyRegistrations { get; set; }

        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public int PendingCount { get; set; }
    }
}
