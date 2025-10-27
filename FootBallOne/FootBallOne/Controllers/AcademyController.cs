using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Controllers
{
    public class AcademyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcademyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // GET: Academy Login Page
        // ============================
        public IActionResult Login()
        {
            HttpContext.Session.Clear(); // Clear any existing session
            return View();
        }

        // ============================
        // POST: Academy Login
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Please enter both email and password.";
                return View();
            }

            var academy = await _context.FootballAcademy
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower() && a.Password == password);

            if (academy != null)
            {
                // Store session info
                HttpContext.Session.SetInt32("AcademyID", academy.AcademyID);
                HttpContext.Session.SetString("AcademyName", academy.AcademyName);

                // Redirect to RegistrationManagements controller, passing AcademyID
                return RedirectToAction("Index", "RegistrationManagements", new { academyId = academy.AcademyID });
            }

            ViewBag.Message = "Invalid Email or Password";
            return View();
        }

        // ============================
        // GET: Academy Profile
        // ============================
        public async Task<IActionResult> Profile()
        {
            var academyId = HttpContext.Session.GetInt32("AcademyID");
            if (!academyId.HasValue)
                return RedirectToAction("Login");

            var academy = await _context.FootballAcademy
                .FirstOrDefaultAsync(a => a.AcademyID == academyId.Value);

            if (academy == null)
                return NotFound("Academy not found.");

            return View(academy);
        }

        // ============================
        // POST: Update Academy Profile
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(FootBallOne.Models.FootballAcademy model)
        {
            var academyId = HttpContext.Session.GetInt32("AcademyID");
            if (!academyId.HasValue)
                return RedirectToAction("Login");

            var academy = await _context.FootballAcademy
                .FirstOrDefaultAsync(a => a.AcademyID == academyId.Value);

            if (academy == null)
                return NotFound("Academy not found.");

            // Update only allowed fields
            academy.AcademyName = model.AcademyName;
            academy.AcademyAddress = model.AcademyAddress;
            academy.Location = model.Location;
            academy.Capacity = model.Capacity;
            academy.ContactNo = model.ContactNo;
            academy.HeadCoach = model.HeadCoach;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ============================
        // GET: Logout
        // ============================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}
