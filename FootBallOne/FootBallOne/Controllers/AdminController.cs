using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

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
            ViewBag.AcademyID = academyId;
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AcademyID,Email,Password,IsSuperAdmin")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                _context.Add(admin);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Admin account created successfully!";
                return RedirectToAction(nameof(Create));
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
            var foundAdmin = _context.Admintbl
                .FirstOrDefault(a => a.Email == admin.Email && a.Password == admin.Password);

            if (foundAdmin != null)
            {
                HttpContext.Session.SetString("AdminEmail", foundAdmin.Email);
                HttpContext.Session.SetInt32("AcademyID", foundAdmin.AcademyID ?? 0);
                return RedirectToAction("Index", "RegistrationManagements");
            }

            ViewBag.Message = "Invalid Email or Password!";
            return View();
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login");

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
                return Unauthorized();

            var players = await _context.Logintbl
                .Where(p => p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View(players);
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Admin Dashboard
        public async Task<IActionResult> AdminDashboard()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login");

            var admin = await _context.Admintbl
                .FirstOrDefaultAsync(a => a.Email == adminEmail);

            if (admin == null)
                return NotFound("Admin details not found.");

            return View(admin);
        }
    }
}
