using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Http;

namespace FootBallOne.Controllers
{
    public class LOGINController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LOGINController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index2()
        {
            return View(await _context.Logintbl.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var login = await _context.Logintbl.FirstOrDefaultAsync(m => m.Id == id);
            if (login == null)
                return NotFound();

            return View(login);
        }

        public IActionResult Create(int? academyId)
        {
            ViewBag.AcademyID = academyId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,Email,Address,City,ImageURL,CoachName,WorkingHours,PaymentStatus")] RegistrationManagement registrationManagement)
        {
            if (ModelState.IsValid)
            {
                int? academyId = HttpContext.Session.GetInt32("AcademyID");
                if (academyId == null || academyId == 0)
                    return Unauthorized();

                registrationManagement.AcademyID = academyId;
                registrationManagement.CreatedDate = DateTime.Now;

                if (registrationManagement.SubscriptionStart < new DateTime(1753, 1, 1))
                    registrationManagement.SubscriptionStart = DateTime.Today;

                if (registrationManagement.SubscriptionEnd < new DateTime(1753, 1, 1))
                    registrationManagement.SubscriptionEnd = DateTime.Today.AddMonths(1);

                // FIXED: Handle nullables correctly
                registrationManagement.TotalFees =
                    registrationManagement.SubscriptionFees.GetValueOrDefault() +
                    registrationManagement.KitFees.GetValueOrDefault() +
                    registrationManagement.SugarFees.GetValueOrDefault() +
                    registrationManagement.BagFees.GetValueOrDefault();

                _context.Add(registrationManagement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(registrationManagement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,Email,Address,City,ImageURL,CoachName,WorkingHours,PaymentStatus,CreatedDate,AcademyID")] RegistrationManagement registrationManagement)
        {
            if (id != registrationManagement.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (registrationManagement.SubscriptionStart < new DateTime(1753, 1, 1))
                        registrationManagement.SubscriptionStart = DateTime.Today;

                    if (registrationManagement.SubscriptionEnd < new DateTime(1753, 1, 1))
                        registrationManagement.SubscriptionEnd = DateTime.Today.AddMonths(1);

                    registrationManagement.TotalFees =
                        registrationManagement.SubscriptionFees.GetValueOrDefault() +
                        registrationManagement.KitFees.GetValueOrDefault() +
                        registrationManagement.SugarFees.GetValueOrDefault() +
                        registrationManagement.BagFees.GetValueOrDefault();

                    _context.Update(registrationManagement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistrationManagementExists(registrationManagement.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(registrationManagement);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var login = await _context.Logintbl.FirstOrDefaultAsync(m => m.Id == id);
            if (login == null)
                return NotFound();

            return View(login);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Logintbl.FindAsync(id);
            if (user != null)
            {
                var userData = $"Name: {user.Name}, Email: {user.Email}, Phone: {user.PhoneNo}";
                System.IO.File.WriteAllText("DeletedUsersLog.txt", userData);

                _context.Logintbl.Remove(user);
                await _context.SaveChangesAsync();

                TempData["Deleted"] = $"User '{user.Name}' was deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LoginExists(int id)
        {
            return _context.Logintbl.Any(e => e.Id == id);
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(string email, string password)
        {
            var user = _context.Logintbl.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                return RedirectToAction("Create", "RegistrationManagements");
            }

            ViewBag.Message = "Invalid Email or Password.";
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
                return RedirectToAction("Login", "Admin");

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
                return Unauthorized();

            var players = await _context.Logintbl
                .Where(p => p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View(players);
        }

        private bool RegistrationManagementExists(int id)
        {
            return _context.RGManagements.Any(e => e.Id == id);
        }
    }
}
