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


        public IActionResult Create(int? academyId)
        {
            ViewBag.AcademyID = academyId; // Pass to view
            return View();
        }


       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AcademyID,Email,Password,AcademyName,CompanyCode,Location,HeadCoach,Capacity,ContactNo,AcademyAddress")] Admin admin)
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
        public IActionResult Login(Admin admin)
        {
            var foundAdmin = _context.Admintbl.FirstOrDefault(a => a.Email == admin.Email && a.Password == admin.Password);
            if (foundAdmin != null)
            {
                HttpContext.Session.SetString("AdminEmail", foundAdmin.Email); // Set session
                HttpContext.Session.SetInt32("AcademyID", foundAdmin.AcademyID ?? 0); // Store AcademyID in session
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

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
                return Unauthorized();

            // Show only players from same AcademyID
            var players = await _context.Logintbl
                .Where(p => p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View(players);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> AdminDashboard()
        {
            // Get the logged-in admin email from session
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login");

            // Fetch the admin details from Admintbl
            var admin = await _context.Admintbl
                .FirstOrDefaultAsync(a => a.Email == adminEmail);

            if (admin == null)
                return NotFound("Admin details not found.");

            return View(admin);
        }


    }
}
