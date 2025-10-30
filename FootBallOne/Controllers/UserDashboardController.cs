using FootBallOne.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootBallOne.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Login Page
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string email, string String)
        {
            var academy = _context.RGManagements.FirstOrDefault(a => a.Email == email && a.Name == String);
            if (academy != null)
            {
                return RedirectToAction("Dashboard", new { id = academy.Id });
            }
            ViewBag.Message = "Invalid Email or Name";
            return View();
        }

        // Academy Dashboard
        public IActionResult Dashboard(int id)
        {
            var academy = _context.RGManagements.FirstOrDefault(a => a.Id == id);
            if (academy == null)
                return NotFound();

            return View(academy);
        }
    }
}
