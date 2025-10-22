using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;

namespace FootBallOne.Controllers
{
    public class AcademyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcademyController(ApplicationDbContext context)
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
        public IActionResult Login(string email, string password)
        {
            var academy = _context.FootballAcademy.FirstOrDefault(a => a.Email == email && a.Password == password);
            if (academy != null)
            {
                return RedirectToAction("Dashboard", new { id = academy.AcademyID });
            }
            ViewBag.Message = "Invalid Email or Password";
            return View();
        }

        // Academy Dashboard
        public IActionResult Dashboard(int id)
        {
            var academy = _context.FootballAcademy.FirstOrDefault(a => a.AcademyID == id);
            if (academy == null)
                return NotFound();

            return View(academy);
        }
    }
}
