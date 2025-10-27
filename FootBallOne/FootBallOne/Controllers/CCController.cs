using FootBallOne.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // ✅ ADDED for Session
using System.Linq;


namespace FootBallOne.Controllers
{
    public class CCController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CCController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================================
        // GET: Login Page
        // =============================================
        public IActionResult Login()
        {
            // Clear any existing session when visiting login page
            HttpContext.Session.Clear();
            return View();
        }

        // =============================================
        // POST: Login by Email and Password
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var player = _context.RGManagements
                .FirstOrDefault(p => p.Email.ToLower() == email.ToLower() && p.Password == password);

            if (player != null)
            {
                // ✅ SAVE USER DATA IN SESSION
                HttpContext.Session.SetInt32("StudentId", player.Id);
                HttpContext.Session.SetString("PlayerName", player.Name ?? "");
                HttpContext.Session.SetString("PlayerEmail", player.Email ?? "");
                HttpContext.Session.SetString("PlayerPhone", player.PhoneNo ?? "");
                HttpContext.Session.SetString("PlayerAddress", player.Address ?? "");
                HttpContext.Session.SetString("PlayerCity", player.City ?? "");

                // Set login success message
                TempData["Success"] = $"Welcome back, {player.Name}!";

                return RedirectToAction("Dashboard", new { id = player.Id });
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // =============================================
        // GET: Player Dashboard
        // =============================================
        public IActionResult Dashboard(int id)
        {
            // Check if user is logged in
            var sessionId = HttpContext.Session.GetInt32("StudentId");

            // Verify that the dashboard being accessed matches the logged-in user
            if (sessionId == null || sessionId != id)
            {
                TempData["Error"] = "Please login to access the dashboard.";
                return RedirectToAction("Login");
            }

            var player = _context.RGManagements.FirstOrDefault(p => p.Id == id);

            if (player == null)
            {
                TempData["Error"] = "Player not found.";
                return RedirectToAction("Login");
            }

            return View(player);
        }

        // =============================================
        // GET: Logout
        // =============================================
        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // =============================================
        // GET: Check Session Status (For Testing)
        // =============================================
        [HttpGet]
        public IActionResult CheckSession()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            var playerName = HttpContext.Session.GetString("PlayerName");
            var playerEmail = HttpContext.Session.GetString("PlayerEmail");

            return Json(new
            {
                IsLoggedIn = studentId.HasValue,
                StudentId = studentId,
                PlayerName = playerName,
                PlayerEmail = playerEmail,
                SessionId = HttpContext.Session.Id,
                Message = studentId.HasValue
                    ? $"Logged in as {playerName} (ID: {studentId})"
                    : "Not logged in"
            });
        }

        // =============================================
        // GET: Quick access to Cart (redirect helper)
        // =============================================
        public IActionResult GoToCart()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                TempData["Error"] = "Please login to access your cart.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Index", "Cart");
        }

        // =============================================
        // GET: Quick access to Products (redirect helper)
        // =============================================
        public IActionResult GoToProducts()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                TempData["Error"] = "Please login to browse products.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Index", "Product");
        }

        // =============================================
        // GET: Profile (View/Edit Profile)
        // =============================================
        public IActionResult Profile()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                TempData["Error"] = "Please login to view your profile.";
                return RedirectToAction("Login");
            }

            var player = _context.RGManagements.FirstOrDefault(p => p.Id == studentId);

            if (player == null)
            {
                TempData["Error"] = "Profile not found.";
                return RedirectToAction("Login");
            }

            return View(player);
        }

        // =============================================
        // POST: Update Profile
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(FootBallOne.Models.RegistrationManagement model)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                TempData["Error"] = "Please login to update your profile.";
                return RedirectToAction("Login");
            }

            var player = _context.RGManagements.FirstOrDefault(p => p.Id == studentId);

            if (player == null)
            {
                TempData["Error"] = "Profile not found.";
                return RedirectToAction("Login");
            }

            // Update only allowed fields
            player.Name = model.Name;
            player.PhoneNo = model.PhoneNo;
            player.Address = model.Address;
            player.City = model.City;
            // Don't allow email change for security

            _context.SaveChanges();

            // Update session with new data
            HttpContext.Session.SetString("PlayerName", player.Name ?? "");
            HttpContext.Session.SetString("PlayerPhone", player.PhoneNo ?? "");
            HttpContext.Session.SetString("PlayerAddress", player.Address ?? "");
            HttpContext.Session.SetString("PlayerCity", player.City ?? "");

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Dashboard", new { id = player.Id });
        }

        // =============================================
        // POST: Change Password
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                return Json(new { success = false, message = "Please login to change password." });
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                return Json(new { success = false, message = "Please fill all fields." });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "New passwords do not match." });
            }

            if (newPassword.Length < 6)
            {
                return Json(new { success = false, message = "Password must be at least 6 characters." });
            }

            var player = _context.RGManagements.FirstOrDefault(p => p.Id == studentId);

            if (player == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (player.Password != currentPassword)
            {
                return Json(new { success = false, message = "Current password is incorrect." });
            }

            player.Password = newPassword;
            _context.SaveChanges();

            return Json(new { success = true, message = "Password changed successfully!" });
        }

        // =============================================
        // GET: Order History
        // =============================================
        public IActionResult OrderHistory()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                TempData["Error"] = "Please login to view your orders.";
                return RedirectToAction("Login");
            }

            // Get orders for this user
            var orders = _context.FBOrders
                .Where(o => o.Id == studentId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        // =============================================
        // Helper method to check if user is logged in
        // =============================================
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("StudentId").HasValue;
        }

        // =============================================
        // Helper method to get current user ID
        // =============================================
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("StudentId") ?? 0;
        }
    }
}