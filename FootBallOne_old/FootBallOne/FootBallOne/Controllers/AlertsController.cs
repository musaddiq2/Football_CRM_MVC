using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootBallOne.Controllers
{
    public class AlertsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlertsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // Paid Players View
        public async Task<IActionResult> Paid()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login", "Admin");

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
                return Unauthorized();

            var paidPlayers = await _context.RGManagements
                .Where(p => p.PaymentStatus == "Paid" && p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View("Paid", paidPlayers); // View name is "Paid.cshtml"
        }

        // Not Paid Players View
        public async Task<IActionResult> NotPaid()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login", "Admin");

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
                return Unauthorized();

            var unpaidPlayers = await _context.RGManagements
                .Where(p => p.PaymentStatus == "Not Paid" && p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View("NotPaid", unpaidPlayers);
        }

        //// Pending Players View
        //public async Task<IActionResult> Pending()
        //{
        //    var adminEmail = HttpContext.Session.GetString("AdminEmail");
        //    if (string.IsNullOrEmpty(adminEmail))
        //        return RedirectToAction("Login", "Admin");

        //    var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
        //    if (admin == null || admin.AcademyID == null)
        //        return Unauthorized();

        //    var pendingPlayers = await _context.RGManagements
        //        .Where(p => p.PaymentStatus == "Pending" && p.AcademyID == admin.AcademyID)
        //        .ToListAsync();

        //    return View("Pending", pendingPlayers);
        //}

        [HttpGet]
        public async Task<IActionResult> Pending()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login", "Admin");

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
                return Unauthorized();

            var pendingPlayers = await _context.RGManagements
                .Where(p => p.PaymentStatus == "Pending" && p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View("Pending", pendingPlayers);
        }

        // ✅ POST: Update Payment Status
        [HttpPost]
        public IActionResult Pending(int id, string newStatus)
        {
            var player = _context.RGManagements.FirstOrDefault(x => x.Id == id);
            if (player != null)
            {
                player.PaymentStatus = newStatus;
                _context.SaveChanges();
                TempData["Updated"] = "Status updated successfully.";
            }
            return RedirectToAction("Pending");
        }




        // 🔸 Show Expired Subscriptions
        public async Task<IActionResult> Expired()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail)) return RedirectToAction("Login", "Admin");

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null) return Unauthorized();

            var today = DateTime.Today;

            var expired = await _context.RGManagements
                .Where(p => p.SubscriptionEnd < today && p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View("Expired", expired); // View: Views/Alerts/Expired.cshtml
        }

        // 🔸 Show Expiring Soon (within 7 days)
        public async Task<IActionResult> ExpiringSoon()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail)) return RedirectToAction("Login", "Admin");

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null) return Unauthorized();

            var today = DateTime.Today;
            var upcoming = today.AddDays(7);

            var expiringSoon = await _context.RGManagements
                .Where(p => p.SubscriptionEnd >= today && p.SubscriptionEnd <= upcoming && p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View("ExpiringSoon", expiringSoon); // View: Views/Alerts/ExpiringSoon.cshtml
        }




        

        // 📍 GET: Alerts/Update/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
                return NotFound();

            var player = await _context.RGManagements.FindAsync(id);
            if (player == null)
                return NotFound();

            return View(player);
        }

        // 📍 POST: Alerts/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string paymentStatus, DateTime? subscriptionEnd)
        {
            var player = await _context.RGManagements.FindAsync(id);
            if (player == null)
                return NotFound();

            if (!string.IsNullOrEmpty(paymentStatus))
                player.PaymentStatus = paymentStatus;

            if (subscriptionEnd.HasValue)
                player.SubscriptionEnd = subscriptionEnd.Value;

            _context.Update(player);
            await _context.SaveChangesAsync();

            TempData["Updated"] = $"{player.Name}'s information updated.";
            return RedirectToAction(nameof(Index));
        }

        // 📍 POST: Alerts/PayOneMonth
        [HttpPost]
        public async Task<IActionResult> PayOneMonth(int id)
        {
            var player = await _context.RGManagements.FirstOrDefaultAsync(p => p.Id == id);
            if (player == null)
                return NotFound();

            // ⏳ Extend subscription
            var baseDate = player.SubscriptionEnd.HasValue && player.SubscriptionEnd > DateTime.Now
         ? player.SubscriptionEnd.Value
         : DateTime.Now;

            player.SubscriptionEnd = baseDate.AddMonths(1);

            player.PaymentStatus = "Paid";

            _context.Update(player);
            await _context.SaveChangesAsync();

            TempData["Updated"] = $"{player.Name}'s subscription extended by 1 month.";
            return RedirectToAction(nameof(Index));
        }


        



    }
}






//using FootBallOne.Data;
//using FootBallOne.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace FootBallOne.Controllers
//{
//    public class AlertsController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public AlertsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            // 1️⃣ Check if Admin is logged in
//            var adminEmail = HttpContext.Session.GetString("AdminEmail");
//            if (string.IsNullOrEmpty(adminEmail))
//            {
//                return RedirectToAction("Login", "Admin");
//            }

//            // 2️⃣ Get logged-in Admin's AcademyID
//            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
//            if (admin == null || admin.AcademyID == null)
//            {
//                return Unauthorized();
//            }

//            var today = DateTime.Today;
//            var upcoming = today.AddDays(7);

//            // 3️⃣ Filter players by Admin’s AcademyID
//            var notPaid = _context.RGManagements
//                .Where(c => c.PaymentStatus == "Not Paid" && c.AcademyID == admin.AcademyID)
//                .ToList();

//            var expired = _context.RGManagements
//                .Where(c => c.SubscriptionEnd < today && c.AcademyID == admin.AcademyID)
//                .ToList();

//            var expiringSoon = _context.RGManagements
//                .Where(c => c.SubscriptionEnd >= today && c.SubscriptionEnd <= upcoming && c.AcademyID == admin.AcademyID)
//                .ToList();

//            // 4️⃣ Pass filtered results to view
//            var viewModel = new AlertsViewModel
//            {
//                NotPaidCoaches = notPaid,
//                ExpiredSubscriptions = expired,
//                ExpiringSoonSubscriptions = expiringSoon
//            };

//            return View(viewModel);
//        }


//        //public IActionResult Index()
//        //{
//        //    var today = DateTime.Today;
//        //    var upcoming = today.AddDays(7);

//        //    var notPaid = _context.RGManagements
//        //        .Where(c => c.PaymentStatus == "Not Paid")
//        //        .ToList();

//        //    var expired = _context.RGManagements
//        //        .Where(c => c.SubscriptionEnd < today)
//        //        .ToList();

//        //    var expiringSoon = _context.RGManagements
//        //        .Where(c => c.SubscriptionEnd >= today && c.SubscriptionEnd <= upcoming)
//        //        .ToList();

//        //    var viewModel = new AlertsViewModel
//        //    {
//        //        NotPaidCoaches = notPaid,
//        //        ExpiredSubscriptions = expired,
//        //        ExpiringSoonSubscriptions = expiringSoon
//        //    };

//        //    return View(viewModel);
//        //}




//        // GET: RegistrationManagements/Update/5
//        public async Task<IActionResult> Update(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var registrationManagement = await _context.RGManagements.FindAsync(id);
//            if (registrationManagement == null)
//            {
//                return NotFound();
//            }

//            return View(registrationManagement);
//        }

//        // POST: RegistrationManagements/Update
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Update(int id, string paymentStatus, DateTime? subscriptionEnd)
//        {
//            var coach = await _context.RGManagements.FindAsync(id);
//            if (coach == null)
//            {
//                return NotFound();
//            }

//            if (!string.IsNullOrEmpty(paymentStatus))
//                coach.PaymentStatus = paymentStatus;

//            if (subscriptionEnd.HasValue)
//                coach.SubscriptionEnd = subscriptionEnd.Value;

//            _context.Update(coach);
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(Index));
//        }

//        [HttpPost]
//        public IActionResult PayOneMonth(int id)
//        {
//            var player = _context.RGManagements .FirstOrDefault(p => p.Id == id);
//            if (player == null)
//            {
//                return NotFound();
//            }

//            // Extend subscription by 1 month from current end date or today
//            var baseDate = player.SubscriptionEnd > DateTime.Now ? player.SubscriptionEnd : DateTime.Now;
//            player.SubscriptionEnd = baseDate.AddMonths(1);

//            _context.SaveChanges();

//            TempData["Updated"] = $"{player.Name}'s subscription extended by 1 month.";
//            return RedirectToAction("Index");
//        }







//    }
//}
