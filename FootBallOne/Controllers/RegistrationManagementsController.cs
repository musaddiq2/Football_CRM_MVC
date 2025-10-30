using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml; // Added for EPPlus
using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;


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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Added for EPPlus
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

        // GET: Create
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

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,Email,Address,City,CoachName,WorkingHours,PaymentStatus,Ground,TimeSlot,BirthDate")] RegistrationManagement registrationManagement, IFormFile imageFile)
        {
            // Recalculate TotalFees
            registrationManagement.TotalFees =
                (registrationManagement.SubscriptionFees ?? 0) +
                (registrationManagement.KitFees ?? 0) +
                (registrationManagement.SugarFees ?? 0) +
                (registrationManagement.BagFees ?? 0);

            // Validate model
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

            // Get AcademyID from session
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

            registrationManagement.AcademyID = academyId;
            registrationManagement.CreatedDate = DateTime.Now;
            registrationManagement.LastUpdated = DateTime.Now;

            // Handle image upload - FIXED VERSION
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    // Ensure uploads folder exists
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var fileExtension = Path.GetExtension(imageFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file to disk
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Save relative path to database
                    registrationManagement.ImageFile = $"/uploads/{fileName}";
                }
                catch (Exception ex)
                {
                    TempData["ModelErrors"] = $"Error uploading image: {ex.Message}";
                    await PopulateViewBagsAsync();
                    return View(registrationManagement);
                }
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
                TempData["ModelErrors"] = $"Error saving to database: {ex.Message}";
                await PopulateViewBagsAsync();
                return View(registrationManagement);
            }
        }

        // GET: Edit
        // GET: Edit - COMPLETE FIXED VERSION
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registrationManagement = await _context.RGManagements
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registrationManagement == null)
            {
                return NotFound();
            }

            // FIX: If AcademyID is missing, populate it from session
            if (registrationManagement.AcademyID == null || registrationManagement.AcademyID == 0)
            {
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

                registrationManagement.AcademyID = academyId;
            }

            // No ViewBag.Coaches needed - using text input instead
            return View(registrationManagement);
        }

        // POST: Edit - FIXED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RegistrationManagement registrationManagement, IFormFile imageFile)
        {
            if (id != registrationManagement.Id)
            {
                return NotFound();
            }

            // Get existing player from database
            var existingPlayer = await _context.RGManagements
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingPlayer == null)
            {
                return NotFound();
            }

            // Remove validation errors for properties that shouldn't be validated
            ModelState.Remove("imageFile");
            ModelState.Remove("ImageFile");
            ModelState.Remove("Step");

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve values that shouldn't change
                    registrationManagement.CreatedDate = existingPlayer.CreatedDate;
                    registrationManagement.LastUpdated = DateTime.Now;

                    // Recalculate TotalFees
                    registrationManagement.TotalFees =
                        (registrationManagement.SubscriptionFees ?? 0) +
                        (registrationManagement.KitFees ?? 0) +
                        (registrationManagement.SugarFees ?? 0) +
                        (registrationManagement.BagFees ?? 0);

                    // Handle new image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        try
                        {
                            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            var fileExtension = Path.GetExtension(imageFile.FileName);
                            var fileName = $"{Guid.NewGuid()}{fileExtension}";
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(stream);
                            }

                            registrationManagement.ImageFile = $"/uploads/{fileName}";

                            // Optional: Delete old image file
                            if (!string.IsNullOrEmpty(existingPlayer.ImageFile))
                            {
                                var oldImagePath = Path.Combine(_env.WebRootPath, existingPlayer.ImageFile.TrimStart('/'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(oldImagePath);
                                    }
                                    catch
                                    {
                                        // Ignore if file can't be deleted
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["ModelErrors"] = $"Error uploading image: {ex.Message}";
                            ViewBag.Coaches = await _context.Coaches
                                .Select(c => new SelectListItem { Value = c.FullName, Text = c.FullName })
                                .ToListAsync();
                            return View(registrationManagement);
                        }
                    }
                    else
                    {
                        // Keep existing image if no new image uploaded
                        registrationManagement.ImageFile = existingPlayer.ImageFile;
                    }

                    // Update the entity
                    _context.Update(registrationManagement);
                    await _context.SaveChangesAsync();

                    //TempData["Updated"] = "Player updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    //if (!RegistrationManagementExists(registrationManagement.Id))
                    //{
                    //    return NotFound();
                    //}
                    //else
                    //{
                    //    throw;
                    //}
                }
                catch (Exception ex)
                {
                    TempData["ModelErrors"] = $"Error updating player: {ex.Message}";
                    ViewBag.Coaches = await _context.Coaches
                        .Select(c => new SelectListItem { Value = c.FullName, Text = c.FullName })
                        .ToListAsync();
                    return View(registrationManagement);
                }
            }
            else
            {
                // Log validation errors
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ModelErrors"] = string.Join(" | ", errors);
            }

            // Reload coaches if model is invalid
            ViewBag.Coaches = await _context.Coaches
                .Select(c => new SelectListItem
                {
                    Value = c.FullName,
                    Text = c.FullName
                })
                .ToListAsync();

            return View(registrationManagement);
        }

        private bool RegistrationManagementExists(int id)
        {
            return _context.RGManagements.Any(e => e.Id == id);
        }

        // DELETE
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

        //private bool RegistrationManagementExists(int id)
        //{
        //    return _context.RGManagements.Any(e => e.Id == id);
        //}

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

        // MODIFIED: Removed authentication checks
        public async Task<IActionResult> Index()
        {
            // Get AcademyID from session if available
            int? academyId = HttpContext.Session.GetInt32("AcademyID");

            // Try admin session as fallback
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

            // If AcademyID exists, filter by it; otherwise show all players
            var players = academyId.HasValue && academyId.Value > 0
            ? await _context.RGManagements.Where(p => p.AcademyID == academyId).ToListAsync()
            : await _context.RGManagements.ToListAsync();

            return View(players);
        }

        // MODIFIED: Removed authentication check
        public IActionResult Dashboard()
        {
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

        // Add these methods to RegistrationManagementsController

        [HttpGet]
        public async Task<IActionResult> RequestRegistration(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound("No invitation token provided.");
            }

            var utcNow = DateTime.UtcNow;
            var invitation = await _context.InvitationLinks
                .Where(i => i.Token == token)
                .Where(i => i.IsActive)
                .Where(i => !i.ExpiresAt.HasValue || i.ExpiresAt > utcNow)
                .Where(i => !i.MaxUsage.HasValue || i.UsageCount < i.MaxUsage.Value)
                .FirstOrDefaultAsync();

            if (invitation == null)
            {
                return NotFound("Invalid or expired invitation link.");
            }

            var model = new PlayerRequest
            {
                InvitationToken = token,
                RequestedDate = DateTime.Now,
                AcademyID = invitation.AcademyID // NEW: Set AcademyID from invitation
            };

            // Pass sharing information to view for social media meta tags and header display
            ViewBag.AcademyID = invitation.AcademyID;
            ViewBag.ShareTitle = invitation.ShareTitle ?? invitation.Name;
            ViewBag.ShareDescription = invitation.ShareDescription ?? invitation.Description;
            ViewBag.ShareImage = invitation.ShareImage;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRegistration(PlayerRequest request)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(request.InvitationToken))
                {
                    ModelState.Remove("InvitationToken");
                }
                else
                {
                    var utcNow = DateTime.UtcNow;
                    var invitation = await _context.InvitationLinks
                        .Where(i => i.Token == request.InvitationToken)
                        .Where(i => i.IsActive)
                        .Where(i => !i.ExpiresAt.HasValue || i.ExpiresAt > utcNow)
                        .Where(i => !i.MaxUsage.HasValue || i.UsageCount < i.MaxUsage.Value)
                        .FirstOrDefaultAsync();

                    if (invitation == null)
                    {
                        ModelState.AddModelError("", "The invitation link is invalid or expired.");
                        return View(request);
                    }

                    // NEW: Set AcademyID from invitation
                    request.AcademyID = invitation.AcademyID;
                    invitation.IncrementUsage();
                }

                _context.PlayerRequests.Add(request);
                request.Status = "Pending";
                request.RequestedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return RedirectToAction("RequestSubmitted");
            }

            return View(request);
        }

        public IActionResult RequestSubmitted()
        {
            return View();
        }

        // NEW: Filter requests by AcademyID
        public async Task<IActionResult> ReviewRequests()
        {
            var academyId = await GetAcademyIdAsync();

            var requestsQuery = _context.PlayerRequests
                .Where(r => r.Status == "Pending");

            // Filter by AcademyID
            if (academyId.HasValue && academyId.Value > 0)
            {
                requestsQuery = requestsQuery.Where(r => r.AcademyID == academyId.Value);
            }

            var requests = await requestsQuery
                .OrderByDescending(r => r.RequestedDate)
                .ToListAsync();

            return View(requests);
        }

        // NEW: Filter rejected requests by AcademyID
        public async Task<IActionResult> RejectedRequests()
        {
            var academyId = await GetAcademyIdAsync();

            var rejectedQuery = _context.PlayerRequests
                .Where(r => r.Status == "Rejected");

            // Filter by AcademyID
            if (academyId.HasValue && academyId.Value > 0)
            {
                rejectedQuery = rejectedQuery.Where(r => r.AcademyID == academyId.Value);
            }

            var rejected = await rejectedQuery
                .OrderByDescending(r => r.RequestedDate)
                .ToListAsync();

            return View(rejected);
        }

        public async Task<IActionResult> AcceptRequest(int id)
        {
            var academyId = await GetAcademyIdAsync();

            var requestQuery = _context.PlayerRequests.AsQueryable();

            // Security: Ensure academy can only accept their own requests
            if (academyId.HasValue && academyId.Value > 0)
            {
                requestQuery = requestQuery.Where(r => r.AcademyID == academyId.Value);
            }

            var request = await requestQuery.FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            var registration = new RegistrationManagement
            {
                Name = request.Name,
                PhoneNo = request.PhoneNo,
                Email = request.Email,
                Address = request.Address,
                City = request.City,
                CreatedDate = DateTime.Now,
                AcademyID = request.AcademyID // Use AcademyID from request
            };

            _context.RGManagements.Add(registration);
            _context.PlayerRequests.Remove(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Player {request.Name} has been accepted and added to registrations.";
            return RedirectToAction("ReviewRequests");
        }

        public async Task<IActionResult> RejectRequest(int id)
        {
            var academyId = await GetAcademyIdAsync();

            var requestQuery = _context.PlayerRequests.AsQueryable();

            // Security: Ensure academy can only reject their own requests
            if (academyId.HasValue && academyId.Value > 0)
            {
                requestQuery = requestQuery.Where(r => r.AcademyID == academyId.Value);
            }

            var request = await requestQuery.FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["InfoMessage"] = $"Request from {request.Name} has been rejected.";
            return RedirectToAction("ReviewRequests");
        }

        // Helper method to get AcademyID
        private async Task<int?> GetAcademyIdAsync()
        {
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

            return academyId;
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

            return View("Create", matchedPlayer);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePlayer(RegistrationManagement model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model);
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

        // GET: SearchSubscriber
        public async Task<IActionResult> SearchSubscriber()
        {
            // Get AcademyID from session if available
            int? academyId = HttpContext.Session.GetInt32("AcademyID");

            // Try admin session as fallback
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

            // If AcademyID exists, filter by it; otherwise show all players
            var players = academyId.HasValue && academyId.Value > 0
                ? await _context.RGManagements.Where(p => p.AcademyID == academyId).ToListAsync()
                : await _context.RGManagements.ToListAsync();

            return View(players);
        }

        public IActionResult ImportSubscribers()
        {
            return View();
        }

        // ... (Other actions remain the same)

        // Updated ExportToExcel with academy filter
        [HttpGet]
        public IActionResult ExportToExcel(string filter = "all")
        {
            // Set EPPlus license explicitly to avoid ambiguity
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Get today's date
            var today = DateTime.Today;

            // Fetch all registrations with Parents
            var registrations = _context.RGManagements
                .Include(r => r.Parents)
                .ToList();

            // Apply Academy filter if present
            int? academyId = HttpContext.Session.GetInt32("AcademyID");
            if (academyId.HasValue && academyId.Value > 0)
            {
                registrations = registrations
                    .Where(r => r.AcademyID == academyId.Value)
                    .ToList();
            }

            // Apply status filter
            switch (filter?.ToLower())
            {
                case "active":
                    registrations = registrations
                        .Where(x => x.SubscriptionStart != null && x.SubscriptionEnd != null
                                    && x.SubscriptionStart <= today && x.SubscriptionEnd >= today)
                        .ToList();
                    break;

                case "inactive":
                    registrations = registrations
                        .Where(x => x.SubscriptionEnd != null && x.SubscriptionEnd < today)
                        .ToList();
                    break;

                case "expiring":
                    registrations = registrations
                        .Where(x => x.SubscriptionEnd != null
                                    && x.SubscriptionEnd >= today
                                    && x.SubscriptionEnd <= today.AddDays(2))
                        .ToList();
                    break;

                case "new":
                    registrations = registrations
                        .Where(x => x.CreatedDate >= today.AddDays(-7))
                        .ToList();
                    break;

                case "all":
                default:
                    break;
            }

            // Map data to export
            var subscribers = registrations.Select(r => new
            {
                r.Id,
                r.Name,
                r.Email,
                r.PhoneNo,
                ParentMobile = r.Parents.FirstOrDefault()?.MobileNo ?? "",
                r.BirthDate,
                r.City,
                r.CreatedDate,
                r.SubscriptionStart,
                r.SubscriptionEnd,
                r.ImageFile
            }).ToList();

            // Generate Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Subscribers");

                // Header
                string[] headers = {
            "ID", "Name", "Email", "PhoneNo", "ParentMobile", "BirthDate",
            "City", "CreatedDate", "SubscriptionStart", "SubscriptionEnd", "ImageFile"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Data rows
                for (int i = 0; i < subscribers.Count; i++)
                {
                    var s = subscribers[i];
                    worksheet.Cells[i + 2, 1].Value = s.Id;
                    worksheet.Cells[i + 2, 2].Value = s.Name;
                    worksheet.Cells[i + 2, 3].Value = s.Email;
                    worksheet.Cells[i + 2, 4].Value = s.PhoneNo;
                    worksheet.Cells[i + 2, 5].Value = s.ParentMobile;
                    worksheet.Cells[i + 2, 6].Value = s.BirthDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 7].Value = s.City;
                    worksheet.Cells[i + 2, 8].Value = s.CreatedDate?.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cells[i + 2, 9].Value = s.SubscriptionStart?.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 10].Value = s.SubscriptionEnd?.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 11].Value = s.ImageFile ?? "";
                }

                worksheet.Cells.AutoFitColumns();

                // Generate file name
                string filterName = filter switch
                {
                    "active" => "Active",
                    "inactive" => "Inactive",
                    "expiring" => "Expiring",
                    "new" => "New",
                    _ => "All"
                };

                string fileName = $"{filterName}_Subscribers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var stream = new MemoryStream(package.GetAsByteArray());

                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }

    }
}