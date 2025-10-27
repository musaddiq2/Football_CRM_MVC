using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FootBallOne.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }


        private bool IsAuthorized()
        {
            return HttpContext.Session.GetString("AdminAccess") == "Granted";
        }


        // GET: Admin/Create
        public IActionResult Create(int? academyId)
        {
            ViewBag.AcademyID = academyId; // Pass to view
            return View();
        }



        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 🟢 FIX: Bind attribute updated to match the corrected Admin model
        public async Task<IActionResult> Create([Bind("Id,AcademyID,Email,Password,IsSuperAdmin")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create", "RegistrationManagements");
            }
            return View(admin);
        }




        // GET: Admin/Login
        public IActionResult Login(int? academyId)
        {
            if (academyId != null)
            {
                ViewBag.SelectedAcademyId = academyId;
            }
            return View();
        }


        // POST: Admin/Login
        [HttpPost]
        public async Task<IActionResult> Login(Admin admin)
        {
            // Validate that branch is selected
            if (admin.AcademyID == null || admin.AcademyID == 0)
            {
                ViewBag.Message = "Please select a branch!";
                return View();
            }

            // Find admin with matching email and password (No more SqlException here)
            var foundAdmin = await _context.Admintbl
                .FirstOrDefaultAsync(a => a.Email == admin.Email && a.Password == admin.Password);

            if (foundAdmin != null)
            {
                // ✅ STORE: Admin Email, AcademyID, and SuperAdmin status in session
                HttpContext.Session.SetString("AdminEmail", foundAdmin.Email);
                HttpContext.Session.SetInt32("AcademyID", admin.AcademyID.Value);
                HttpContext.Session.SetString("IsSuperAdmin", foundAdmin.IsSuperAdmin ? "true" : "false");

                // ✅ STORE: Get and store Branch Name (and other relevant academy details if needed for a quick reference)
                var academy = await _context.FootballAcademy
                    .FirstOrDefaultAsync(a => a.AcademyID == admin.AcademyID.Value);

                if (academy != null)
                {
                    HttpContext.Session.SetString("AcademyName", academy.AcademyName);
                }
                else
                {
                    HttpContext.Session.SetString("AcademyName", "Unknown Branch");
                }

                // Redirect to dashboard or registration page
                return RedirectToAction("Index", "RegistrationManagements");
            }

            ViewBag.Message = "Invalid Email or Password!";
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login");

            // Use the email from the session to retrieve the Admin details
            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);

            int? sessionAcademyID = HttpContext.Session.GetInt32("AcademyID");

            if (admin == null || sessionAcademyID == null)
                return Unauthorized();

            // Show only players from the selected AcademyID stored in the session
            var players = await _context.Logintbl
                .Where(p => p.AcademyID == sessionAcademyID)
                .ToListAsync();

            return View(players);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // 🟢 FIX: Updated AdminDashboard action to fetch Academy details for the view
        public async Task<IActionResult> AdminDashboard()
        {
            // Get the logged-in admin email from session
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login");

            // Fetch the admin details from Admintbl (This is the Model we will pass)
            var admin = await _context.Admintbl
                .FirstOrDefaultAsync(a => a.Email == adminEmail);

            if (admin == null)
                return NotFound("Admin details not found.");

            // ✅ Fetch Academy details using the Admin's AcademyID
            var academy = await _context.FootballAcademy
                .FirstOrDefaultAsync(a => a.AcademyID == admin.AcademyID);

            // ✅ Pass Academy details using ViewBag so the view can display them
            if (academy != null)
            {
                ViewBag.AcademyName = academy.AcademyName;
                ViewBag.CompanyCode = academy.CompanyCode;
                ViewBag.Location = academy.Location;
                ViewBag.HeadCoach = academy.HeadCoach;
                ViewBag.Capacity = academy.Capacity;
                ViewBag.ContactNo = academy.ContactNo;
                ViewBag.AcademyAddress = academy.AcademyAddress;
            }
            else
            {
                // Set default values if academy is not found to prevent null reference exceptions in the view
                ViewBag.AcademyName = "N/A";
                ViewBag.CompanyCode = "N/A";
                ViewBag.Location = "N/A";
                ViewBag.HeadCoach = "N/A";
                ViewBag.Capacity = 0;
                ViewBag.ContactNo = null;
                ViewBag.AcademyAddress = "N/A";
            }

            return View(admin); // Pass the Admin model
        }
    }
}