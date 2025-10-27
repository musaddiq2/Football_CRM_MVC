using FootBallOne.Data;
using Microsoft.AspNetCore.Mvc;
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

        // GET: Login Page
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login by Email and Password
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
                return RedirectToAction("Dashboard", new { id = player.Id });
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // GET: Player Dashboard
        public IActionResult Dashboard(int id)
        {
            var player = _context.RGManagements.FirstOrDefault(p => p.Id == id);
            if (player == null)
            {
                return NotFound();
            }
            return View(player);
        }
    }
}
