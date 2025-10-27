using FootBallOne.Data;
using FootBallOne.Models;
using FootBallOne.Services;
using FootBallOne.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FootBallOne.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context, IAcademyContext academyContext)
       : base(academyContext)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var playersQuery = _context.RGManagements.AsQueryable();

            if (!IsSuperAdmin && CurrentAcademyId.HasValue)
            {
                playersQuery = playersQuery.Where(p => p.AcademyID == CurrentAcademyId);
            }

            var players = playersQuery.ToList();
            var totalPlayers = players.Count;

            var totalPaidPlayers = players.Count(p => p.PaymentStatus != null && p.PaymentStatus.Trim().ToLower() == "paid");
            var totalUnpaidPlayers = players.Count(p => p.PaymentStatus != null && p.PaymentStatus.Trim().ToLower() == "unpaid");
            var totalPendingPlayers = players.Count(p => p.PaymentStatus != null && p.PaymentStatus.Trim().ToLower() == "pending");

            var expiringSubscriptions = players.Count(p => p.SubscriptionEnd != null &&
                                                           p.SubscriptionEnd.Value >= DateTime.Today &&
                                                           p.SubscriptionEnd.Value <= DateTime.Today.AddDays(7));

            var monthlyData = players
                .Where(p => p.CreatedDate != null)
                .GroupBy(p => new { Year = p.CreatedDate.Value.Year, Month = p.CreatedDate.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month)} {g.Key.Year}",
                    Count = g.Count()
                }).ToList();

            var model = new DashboardViewModel
            {
                TotalPlayers = totalPlayers,
                TotalPaidPlayers = totalPaidPlayers,
                TotalUnpaidPlayers = totalUnpaidPlayers,
                TotalCoaches = _context.Coaches.Count(),
                NewPlayerRequests = totalPendingPlayers,
                ExpiringSubscriptions = expiringSubscriptions,
                TotalRevenue = totalPaidPlayers * 1000,
                MonthlyLabels = monthlyData.Select(m => m.Month).ToList(),
                MonthlyRegistrations = monthlyData.Select(m => m.Count).ToList(),
                PaidCount = totalPaidPlayers,
                UnpaidCount = totalUnpaidPlayers,
                PendingCount = totalPendingPlayers
            };

            return View(model);
        }
    }
}
