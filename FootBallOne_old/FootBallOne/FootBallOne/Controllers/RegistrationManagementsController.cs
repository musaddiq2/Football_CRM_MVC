using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;
using FootBallOne.Models;

namespace FootBallOne.Controllers
{
    public class RegistrationManagementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RegistrationManagementsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // STEP 1 - Enter Email or Phone
        public IActionResult Step1()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Step1(string email, string phoneNo)
        {
            TempData["Email"] = email;
            TempData["PhoneNo"] = phoneNo;
            return RedirectToAction("Step2");
        }

        // STEP 2 - Fill full form
        public IActionResult Step2()
        {
            var model = new RegistrationManagement
            {
                Email = TempData["Email"]?.ToString(),
                PhoneNo = TempData["PhoneNo"]?.ToString()
            };
            TempData.Keep();
            return View(model);
        }

        [HttpPost]
        public IActionResult Step2(RegistrationManagement model)
        {
            TempData["FormData"] = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            return RedirectToAction("Step3");
        }

        // STEP 3 - Summary
        public IActionResult Step3()
        {
            var json = TempData["FormData"]?.ToString();
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Step1");

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<RegistrationManagement>(json);
            TempData.Keep();
            return View(model);
        }

        [HttpPost]
        public IActionResult Submit()
        {
            var json = TempData["FormData"]?.ToString();
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Step1");

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<RegistrationManagement>(json);
            _context.RGManagements.Add(model);
            _context.SaveChanges();

            TempData.Clear();
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }

        // GET: RegistrationManagements
        public async Task<IActionResult> Index2()
        {
            return View(await _context.RGManagements.ToListAsync());
        }

        // GET: RegistrationManagements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registrationManagement = await _context.RGManagements
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registrationManagement == null)
            {
                return NotFound();
            }

            return View(registrationManagement);
        }




        // GET: Create (unchanged aside from using helper)
        public async Task<IActionResult> Create()
        {
            await PopulateViewBagsAsync();
            return View();
        }

        private async Task PopulateViewBagsAsync()
        {
            ViewBag.AcceptedPlayers = await _context.RGManagements
                .Where(p => p.Name != null)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name ?? "Unnamed Player"
                }).ToListAsync();

            ViewBag.Coaches = await _context.Coaches
                .Where(c => c.FullName != null)
                .Select(c => new SelectListItem
                {
                    Value = c.FullName,
                    Text = c.FullName
                }).ToListAsync();
        }

        // POST: Create (updated)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,Email,Address,City,ImageURL,CoachName,WorkingHours,PaymentStatus,Ground,TimeSlot,BirthDate")] RegistrationManagement registrationManagement, IFormFile imageFile)
        {
            // Recalculate TotalFees here too (UX safety)
            registrationManagement.TotalFees =
                (registrationManagement.SubscriptionFees ?? 0) +
                (registrationManagement.KitFees ?? 0) +
                (registrationManagement.SugarFees ?? 0) +
                (registrationManagement.BagFees ?? 0);

            // If modelstate invalid -> show combined errors to TempData so view can show it
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value.Errors.Select(err => $"{kvp.Key}: {err.ErrorMessage}"))
                    .ToList();

                TempData["ModelErrors"] = string.Join(" | ", allErrors);
                await PopulateViewBagsAsync();
                return View(registrationManagement);
            }

            // Get AcademyID: prefer session "AcademyID", fallback to admin session if present
            int? academyId = HttpContext.Session.GetInt32("AcademyID");
            if (academyId == null || academyId == 0)
            {
                var adminEmail = HttpContext.Session.GetString("AdminEmail");
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
                    if (admin != null && admin.AcademyID != null)
                        academyId = admin.AcademyID;
                }
            }

            if (academyId == null || academyId == 0)
            {
                // Better UX than Unauthorized() — show helpful message
                TempData["ModelErrors"] = "Academy information not found in session. Please login as admin or set AcademyID in session.";
                await PopulateViewBagsAsync();
                return View(registrationManagement);
            }

            registrationManagement.AcademyID = academyId;
            registrationManagement.CreatedDate = DateTime.Now;
            registrationManagement.LastUpdated = DateTime.Now;

            // Handle image upload (optional) -> save to wwwroot/uploads and set ImageURL
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // store relative path
                registrationManagement.ImageFile = "/uploads/" + fileName;
            }

            try
            {
                _context.Add(registrationManagement);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Player registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log ex (use your logger in real app). For now show message to user.
                TempData["ModelErrors"] = "Error saving to database: " + ex.Message;
                await PopulateViewBagsAsync();
                return View(registrationManagement);
            }
        }

        //// GET: RegistrationManagements/Create
        //public async Task<IActionResult> Create()
        //{
        //    ViewBag.AcceptedPlayers = await _context.RGManagements
        //        .Where(p => p.Name != null) // Optional: Filter out rows with null Name
        //        .Select(p => new SelectListItem
        //        {
        //            Value = p.Id.ToString(),
        //            Text = p.Name ?? "Unnamed Player"
        //        }).ToListAsync();

        //    ViewBag.Coaches = _context.Coaches
        //        .Where(c => c.FullName != null)
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.FullName,
        //            Text = c.FullName
        //        }).ToList();

        //    return View();
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,Email,Address,City,ImageURL,CoachName,WorkingHours,PaymentStatus,Ground,TimeSlot,BirthDate")] RegistrationManagement registrationManagement)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        int? academyId = HttpContext.Session.GetInt32("AcademyID");
        //        if (academyId == null || academyId == 0)
        //        {
        //            return Unauthorized();
        //        }

        //        registrationManagement.AcademyID = academyId;
        //        registrationManagement.CreatedDate = DateTime.Now;
        //        registrationManagement.LastUpdated = DateTime.Now;

        //        registrationManagement.TotalFees =
        //            (registrationManagement.SubscriptionFees ?? 0) +
        //            (registrationManagement.KitFees ?? 0) +
        //            (registrationManagement.SugarFees ?? 0) +
        //            (registrationManagement.BagFees ?? 0);

        //        _context.Add(registrationManagement);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    // Repopulate dropdowns
        //    ViewBag.Coaches = _context.Coaches
        //        .Where(c => c.FullName != null)
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.FullName,
        //            Text = c.FullName
        //        }).ToList();

        //    ViewBag.AcceptedPlayers = await _context.RGManagements
        //        .Where(p => p.Name != null)
        //        .Select(p => new SelectListItem
        //        {
        //            Value = p.Id.ToString(),
        //            Text = p.Name ?? "Unnamed Player"
        //        }).ToListAsync();

        //    return View(registrationManagement);
        //}



        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var adminEmail = HttpContext.Session.GetString("AdminEmail");
        //    if (string.IsNullOrEmpty(adminEmail))
        //    {
        //        return RedirectToAction("Login", "Admin");
        //    }

        //    var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
        //    if (admin == null || admin.AcademyID == null)
        //    {
        //        return Unauthorized();
        //    }

        //    var registrationManagement = await _context.RGManagements
        //        .FirstOrDefaultAsync(r => r.Id == id && r.AcademyID == admin.AcademyID);

        //    if (registrationManagement == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(registrationManagement);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,AcademyID,Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,TotalFees,Email,Password,Address,City,ImageURL,CoachName,WorkingHours,PaymentStatus,CreatedDate,LastUpdated,Ground,TimeSlot,BirthDate")] RegistrationManagement registrationManagement)
        //{
        //    var adminEmail = HttpContext.Session.GetString("AdminEmail");
        //    if (string.IsNullOrEmpty(adminEmail))
        //    {
        //        return RedirectToAction("Login", "Admin");
        //    }

        //    var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
        //    if (admin == null || admin.AcademyID == null)
        //    {
        //        return Unauthorized();
        //    }

        //    if (id != registrationManagement.Id)
        //    {
        //        return NotFound();
        //    }

        //    // Ensure the player belongs to the same academy
        //    var existingPlayer = await _context.RGManagements
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(r => r.Id == id && r.AcademyID == admin.AcademyID);

        //    if (existingPlayer == null)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            registrationManagement.LastUpdated = DateTime.Now;
        //            registrationManagement.AcademyID = admin.AcademyID; // reassign to ensure AcademyID is correct

        //            registrationManagement.TotalFees =
        //                (registrationManagement.SubscriptionFees ?? 0) +
        //                (registrationManagement.KitFees ?? 0) +
        //                (registrationManagement.SugarFees ?? 0) +
        //                (registrationManagement.BagFees ?? 0);

        //            _context.Update(registrationManagement);
        //            await _context.SaveChangesAsync();

        //            TempData["Updated"] = "Player updated successfully!";
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!RegistrationManagementExists(registrationManagement.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //    }

        //    return View(registrationManagement);
        //}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Admin");
            }

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
            {
                return Unauthorized();
            }

            var registrationManagement = await _context.RGManagements
                .FirstOrDefaultAsync(r => r.Id == id && r.AcademyID == admin.AcademyID);

            if (registrationManagement == null)
            {
                return NotFound();
            }

            // ✅ Get all coaches (no AcademyID filter)
            ViewBag.Coaches = await _context.Coaches
                .Select(c => new SelectListItem
                {
                    Value = c.FullName,
                    Text = c.FullName
                })
                .ToListAsync();

            return View(registrationManagement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AcademyID,Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,TotalFees,Email,Password,Address,City,ImageURL,CoachName,WorkingHours,PaymentStatus,CreatedDate,LastUpdated,Ground,TimeSlot,BirthDate")] RegistrationManagement registrationManagement)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Admin");
            }

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
            {
                return Unauthorized();
            }

            if (id != registrationManagement.Id)
            {
                return NotFound();
            }

            var existingPlayer = await _context.RGManagements
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.AcademyID == admin.AcademyID);

            if (existingPlayer == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    registrationManagement.LastUpdated = DateTime.Now;
                    registrationManagement.AcademyID = admin.AcademyID;

                    registrationManagement.TotalFees =
                        (registrationManagement.SubscriptionFees ?? 0) +
                        (registrationManagement.KitFees ?? 0) +
                        (registrationManagement.SugarFees ?? 0) +
                        (registrationManagement.BagFees ?? 0);

                    _context.Update(registrationManagement);
                    await _context.SaveChangesAsync();

                    TempData["Updated"] = "Player updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistrationManagementExists(registrationManagement.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // ✅ Reload all coaches if model is invalid
            ViewBag.Coaches = await _context.Coaches
                .Select(c => new SelectListItem
                {
                    Value = c.FullName,
                    Text = c.FullName
                })
                .ToListAsync();

            return View(registrationManagement);
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registrationManagement = await _context.RGManagements
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registrationManagement == null)
            {
                return NotFound();
            }

            return View(registrationManagement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registrationManagement = await _context.RGManagements.FindAsync(id);
            if (registrationManagement != null)
            {
                _context.RGManagements.Remove(registrationManagement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegistrationManagementExists(int id)
        {
            return _context.RGManagements.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registrationManagement = await _context.RGManagements.FindAsync(id);
            if (registrationManagement == null)
            {
                return NotFound();
            }

            return View(registrationManagement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string paymentStatus, DateTime? subscriptionEnd)
        {
            var coach = await _context.RGManagements.FindAsync(id);
            if (coach == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(paymentStatus))
                coach.PaymentStatus = paymentStatus;

            if (subscriptionEnd.HasValue)
                coach.SubscriptionEnd = subscriptionEnd.Value;

            _context.Update(coach);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Payment(int playerId, string playerName)
        {
            ViewBag.PlayerId = playerId;
            ViewBag.PlayerName = playerName;
            return View();
        }

        [HttpPost]
        public IActionResult Confirm(int playerId, string playerName, string paymentMethod)
        {
            var player = _context.RGManagements.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.SubscriptionEnd = (player.SubscriptionEnd ?? DateTime.Today).AddMonths(1);
                _context.SaveChanges();
            }

            ViewBag.PaymentSuccess = $"✅ Payment via {paymentMethod} for {playerName} was successful!";
            ViewBag.PlayerId = playerId;
            ViewBag.PlayerName = playerName;

            return View("Index");
        }

        public async Task<IActionResult> Index()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Admin");
            }

            var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
            if (admin == null || admin.AcademyID == null)
            {
                return Unauthorized();
            }

            var players = await _context.RGManagements
                .Where(p => p.AcademyID == admin.AcademyID)
                .ToListAsync();

            return View(players);
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("admin") == null)
                return RedirectToAction("Login", "LOGIN");

            var totalPlayers = _context.RGManagements.Count();
            var paidPlayers = _context.RGManagements.Count(p => p.PaymentStatus == "Paid");
            var unpaidPlayers = _context.RGManagements.Count(p => p.PaymentStatus == "Unpaid");
            var pendingPlayers = _context.RGManagements.Count(p => p.PaymentStatus == "Pending");
            var totalCoaches = _context.Coaches.Select(p => p.FullName).Distinct().Count();
            var alerts = _context.RGManagements.Count(p => p.SubscriptionEnd <= DateTime.Today.AddDays(7));

            ViewBag.TotalPlayers = totalPlayers;
            ViewBag.PaidPlayers = paidPlayers;
            ViewBag.UnpaidPlayers = unpaidPlayers;
            ViewBag.PendingPlayers = pendingPlayers;
            ViewBag.TotalCoaches = totalCoaches;
            ViewBag.Alerts = alerts;

            return View();
        }

        [HttpGet]
        public IActionResult RequestRegistration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRegistration(PlayerRequest request)
        {

            if (ModelState.IsValid)
            {
                _context.PlayerRequests.Add(request);
                request.Status = "Pending";

                await _context.SaveChangesAsync();
                return RedirectToAction("RequestSubmitted");
            }

            return View(request);
        }

        public IActionResult RequestSubmitted()
        {
            return View();
        }

        public async Task<IActionResult> ReviewRequests()
        {
            var requests = await _context.PlayerRequests.ToListAsync();
            return View(requests);
        }

        public async Task<IActionResult> AcceptRequest(int id)
        {
            var request = await _context.PlayerRequests.FindAsync(id);
            if (request == null) return NotFound();

            var registration = new RegistrationManagement
            {
                Name = request.Name,
                PhoneNo = request.PhoneNo,
                Email = request.Email,
                Address = request.Address,
                City = request.City,
                CreatedDate = DateTime.Now,
                AcademyID = HttpContext.Session.GetInt32("AcademyID")
            };

            _context.RGManagements.Add(registration);
            _context.PlayerRequests.Remove(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("ReviewRequests");
        }

        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.PlayerRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "Rejected"; // ← set status instead of deleting
            await _context.SaveChangesAsync();

            return RedirectToAction("ReviewRequests");
        }

        public async Task<IActionResult> RejectedRequests()
        {
            var rejected = await _context.PlayerRequests
                .Where(r => r.Status == "Rejected")
                .ToListAsync();

            return View(rejected);
        }




        [HttpGet]
        public async Task<IActionResult> GetPlayerDetails(int id)
        {
            var player = await _context.RGManagements
                .Where(p => p.Id == id)
                .Select(p => new {
                    name = p.Name,
                    phoneNo = p.PhoneNo,
                    email = p.Email,
                    address = p.Address,
                    city = p.City
                })
                .FirstOrDefaultAsync();

            if (player == null) return NotFound();
            return Json(player);
        }

        [HttpPost]
        public async Task<IActionResult> SearchPlayer(string phoneOrEmail)
        {
            var matchedPlayer = await _context.RGManagements
                .FirstOrDefaultAsync(p => p.PhoneNo == phoneOrEmail || p.Email == phoneOrEmail);

            if (matchedPlayer == null)
            {
                TempData["ErrorMessage"] = "No player found with the given phone or email.";
                return RedirectToAction("Create");
            }

            return View("Create", matchedPlayer); // This will return Create view with data prefilled
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePlayer(RegistrationManagement model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model); // Show errors
            }

            var existing = await _context.RGManagements.FindAsync(model.Id);
            if (existing == null)
            {
                return NotFound();
            }

            // Update properties
            existing.Name = model.Name;
            existing.Email = model.Email;
            existing.PhoneNo = model.PhoneNo;
            existing.Address = model.Address;
            existing.City = model.City;
            existing.ImageFile = model.ImageFile;
            existing.PaymentStatus = model.PaymentStatus;
            existing.SubscriptionEnd = model.SubscriptionEnd;
            existing.CoachName = model.CoachName;
            existing.WorkingHours = model.WorkingHours;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Player updated successfully!";
            return RedirectToAction("Create");
        }




    }
}
