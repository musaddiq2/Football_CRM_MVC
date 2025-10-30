using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: FootballAcademy/Create
        public IActionResult CreateAcademy()
        {
            return View();
        }

        // POST: FootballAcademy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAcademy(FootballAcademy academy)
        {
            if (ModelState.IsValid)
            {
                academy.IsActive = true;
                academy.CreatedDate = DateTime.Now;
                _context.Add(academy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Football Academy created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(academy);
        }

        // GET: Admin/Create
        public IActionResult CreateAdmin(int? academyId)
        {
            ViewBag.AcademyID = academyId; // Pass to view
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin([Bind("Id,AcademyID,Email,Password,IsSuperAdmin")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "RegistrationManagements");
            }
            return View(admin);
        }

        // GET: Academy/Index
        public async Task<IActionResult> Index()
        {
            // Check if user is SuperAdmin
            if (HttpContext.Session.GetString("UserRole") != "SuperAdmin")
            {
                TempData["Error"] = "Access Denied: Only Super Admins can view the academy list.";
                return RedirectToAction("Login", "Admin");
            }

            try
            {
                var academies = await _context.FootballAcademy
                    
                    .OrderBy(a => a.AcademyName)
                    .ToListAsync();
                return View(academies);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the academies.";
                System.Diagnostics.Debug.WriteLine($"Error in Index: {ex.Message}");
                return View(new List<Academy>());
            }
        }

        // GET: Academy/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> EditAcademy(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "SuperAdmin")
            {
                TempData["Error"] = "Access Denied: Only Super Admins can edit academies.";
                return RedirectToAction("Login", "Admin");
            }

            var academy = await _context.FootballAcademy.FindAsync(id);
            if (academy == null)
            {
                TempData["Error"] = "Academy not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(academy);
        }

        // POST: Academy/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAcademy(int id, Academy academy)
        {
            if (HttpContext.Session.GetString("UserRole") != "SuperAdmin")
            {
                TempData["Error"] = "Access Denied: Only Super Admins can edit academies.";
                return RedirectToAction("Login", "Admin");
            }

            if (id != academy.AcademyID)
            {
                TempData["Error"] = "Invalid academy ID.";
                return RedirectToAction(nameof(Index));
            }

            // Remove Password validation error if the field is blank
            if (string.IsNullOrEmpty(academy.Password))
            {
                ModelState.Remove("Password");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAcademy = await _context.FootballAcademy.FindAsync(id);
                    if (existingAcademy == null)
                    {
                        TempData["Error"] = "Academy not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update fields
                    existingAcademy.AcademyName = academy.AcademyName;
                    existingAcademy.Location = academy.Location;
                    existingAcademy.CompanyCode = academy.CompanyCode;
                    existingAcademy.HeadCoach = academy.HeadCoach;
                    existingAcademy.Capacity = academy.Capacity;
                    existingAcademy.ContactNo = academy.ContactNo;
                    existingAcademy.AcademyAddress = academy.AcademyAddress;
                    existingAcademy.Email = academy.Email;

                    // Only update password if a new value is provided
                    if (!string.IsNullOrEmpty(academy.Password))
                    {
                        existingAcademy.Password = academy.Password;
                    }

                    _context.Update(existingAcademy);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Academy updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while updating the academy.";
                    System.Diagnostics.Debug.WriteLine($"Error in EditAcademy: {ex.Message}");
                }
            }

            return View(academy);
        }

        // POST: Academy/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAcademy(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "SuperAdmin")
            {
                TempData["Error"] = "Access Denied: Only Super Admins can delete academies.";
                return RedirectToAction("Login", "Admin");
            }

            var academy = await _context.FootballAcademy.FindAsync(id);
            if (academy == null)
            {
                TempData["Error"] = "Academy not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
               
                _context.Update(academy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Academy deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the academy.";
                System.Diagnostics.Debug.WriteLine($"Error in DeleteAcademy: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
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
            // STEP 1: Try to authenticate as Super Admin first (Admintbl)
            var foundAdmin = await _context.Admintbl
                .FirstOrDefaultAsync(a => a.Email == admin.Email && a.Password == admin.Password);
            if (foundAdmin != null)
            {
                // Super Admin authentication successful
                HttpContext.Session.SetString("AdminEmail", foundAdmin.Email);
                HttpContext.Session.SetString("IsSuperAdmin", "true");
                HttpContext.Session.SetString("UserRole", "SuperAdmin");
                // Set default branch for Super Admin
                var defaultAcademy = await _context.FootballAcademy.FirstOrDefaultAsync();
                if (defaultAcademy != null)
                {
                    HttpContext.Session.SetInt32("AcademyID", defaultAcademy.AcademyID);
                    HttpContext.Session.SetString("AcademyName", defaultAcademy.AcademyName);
                }
                else
                {
                    // Fallback if no academies exist
                    HttpContext.Session.SetInt32("AcademyID", 1);
                    HttpContext.Session.SetString("AcademyName", "Default Branch");
                }
                return RedirectToAction("Index", "RegistrationManagements");
            }

            // STEP 2: Try Manager authentication (FootballAcademy)
            var foundManager = await _context.FootballAcademy
                .FirstOrDefaultAsync(a => a.Email == admin.Email && a.Password == admin.Password);
            if (foundManager != null)
            {
                // Manager authentication successful
                HttpContext.Session.SetString("AdminEmail", foundManager.Email);
                HttpContext.Session.SetString("IsSuperAdmin", "false");
                HttpContext.Session.SetString("UserRole", "Manager");
                HttpContext.Session.SetInt32("AcademyID", foundManager.AcademyID);
                HttpContext.Session.SetString("AcademyName", foundManager.AcademyName);
                // Store Manager's fixed academy ID (cannot be changed)
                HttpContext.Session.SetInt32("ManagerAcademyID", foundManager.AcademyID);
                return RedirectToAction("Index", "RegistrationManagements");
            }

            // Both authentications failed
            ViewBag.Message = "Invalid Email or Password!";
            return View();
        }

        // GET: Admin/Dashboard
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

        // GET: Admin/AdminDashboard
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

            // Fetch Academy details using the Admin's AcademyID
            var academy = await _context.FootballAcademy
                .FirstOrDefaultAsync(a => a.AcademyID == admin.AcademyID);

            // Pass Academy details using ViewBag
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
                // Set default values if academy not found
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

        // POST: Admin/SwitchBranch
        [HttpPost]
        public async Task<IActionResult> SwitchBranch([FromBody] BranchSwitchRequest request)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            // Check if user is a Manager - managers cannot switch branches
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Manager")
            {
                return Json(new { success = false, message = "Managers cannot switch branches" });
            }

            if (request == null || request.academyId <= 0)
            {
                return Json(new { success = false, message = "Invalid branch ID" });
            }

            var academy = await _context.FootballAcademy
                .FirstOrDefaultAsync(a => a.AcademyID == request.academyId);
            if (academy == null)
            {
                // Log for debugging
                var allAcademies = await _context.FootballAcademy.ToListAsync();
                System.Diagnostics.Debug.WriteLine($"Academy not found. Looking for ID: {request.academyId}");
                System.Diagnostics.Debug.WriteLine($"Available academies: {string.Join(", ", allAcademies.Select(a => $"{a.AcademyID}:{a.AcademyName}"))}");
                return Json(new { success = false, message = $"Branch with ID {request.academyId} not found" });
            }

            // Update session with new branch (only for Super Admin)
            HttpContext.Session.SetInt32("AcademyID", academy.AcademyID);
            HttpContext.Session.SetString("AcademyName", academy.AcademyName);
            return Json(new
            {
                success = true,
                message = "Branch switched successfully",
                academyName = academy.AcademyName,
                academyId = academy.AcademyID
            });
        }

        // GET: Admin/GetBranches
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var branches = await _context.FootballAcademy
                    .OrderBy(a => a.AcademyName)
                    .Select(a => new
                    {
                        academyID = a.AcademyID,
                        academyName = a.AcademyName,
                        location = a.Location
                    })
                    .ToListAsync();

                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"Loaded {branches.Count} branches");
                if (!branches.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No branches found in database",
                        branches = new List<object>()
                    });
                }

                return Json(new { success = true, branches });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading branches: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "Error loading branches: " + ex.Message
                });
            }
        }

        // POST: Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Request model for branch switching
        public class BranchSwitchRequest
        {
            public int academyId { get; set; }
        }
    }
}