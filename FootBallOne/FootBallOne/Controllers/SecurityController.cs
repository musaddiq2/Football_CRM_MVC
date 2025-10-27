// Controllers/SecurityController.cs
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace FootBallOne.Controllers
{
    public class SecurityController : Controller
    {
        // Replace this with your own logic or DB-stored value
        private const string AdminPassword = "admin123"; // change this!

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(SecurityAccessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password == AdminPassword)
            {
                HttpContext.Session.SetString("AdminAccess", "Granted");
                return RedirectToAction("Create", "Admin"); // redirect to your admin dashboard
            }

            ModelState.AddModelError(string.Empty, "Incorrect password.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminAccess");
            return RedirectToAction("Index");
        }
    }
}
