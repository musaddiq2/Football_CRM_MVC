using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;
using FootBallOne.Models;

namespace FootBallOne.Controllers
{
    public class ParentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParentsController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ParentsController(ApplicationDbContext context, ILogger<ParentsController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Parents/Index
        public async Task<IActionResult> Index(int? registrationId = null)
        {
            try
            {
                var parents = await _context.Parents
                    .Include(p => p.PhoneNumbers)
                    .OrderBy(p => p.Fullname)
                    .ToListAsync();

                // Get all students and create a dictionary for lookup
                var students = await _context.RGManagements.ToListAsync();
                var studentDictionary = students.ToDictionary(s => s.Id, s => s.Name);

                ViewBag.Students = studentDictionary;

                // If viewing for specific student
                if (registrationId.HasValue)
                {
                    parents = parents.Where(p => p.RegistrationId == registrationId.Value).ToList();
                    var student = students.FirstOrDefault(s => s.Id == registrationId.Value);
                    ViewBag.StudentName = student?.Name ?? "Unknown";
                    ViewBag.RegistrationId = registrationId.Value;
                }

                return View(parents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading parents list");
                TempData["ErrorMessage"] = "Error loading parents";
                return RedirectToAction("Index", "RegistrationManagement");
            }
        }   

        // GET: Parents/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var students = await _context.RGManagements
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                ViewBag.Students = students;
                return View(new ParentCreateViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                TempData["ErrorMessage"] = "Error loading form";
                return RedirectToAction("Index");
            }
        }

        // POST: Parents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParentCreateViewModel model, IFormFile SubscriberPhoto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var students = await _context.RGManagements.OrderBy(r => r.Name).ToListAsync();
                    ViewBag.Students = students;
                    return View(model);
                }

                // Handle file upload
                string photoPath = null;
                if (SubscriberPhoto != null && SubscriberPhoto.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(SubscriberPhoto.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("SubscriberPhoto", "Only image files (jpg, jpeg, png, gif) are allowed.");
                        var students = await _context.RGManagements.OrderBy(r => r.Name).ToListAsync();
                        ViewBag.Students = students;
                        return View(model);
                    }

                    // Create unique filename
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "parents");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await SubscriberPhoto.CopyToAsync(fileStream);
                    }

                    // Store relative path for database
                    photoPath = $"/uploads/parents/{fileName}";
                }

                // Create Parent
                var parent = new Parent
                {
                    Fullname = model.Fullname,
                    Email = model.Email,
                    MobileNo = model.MobileNo,
                    Password = model.Password,
                    Gender = model.Gender,
                    IqamaID = model.IqamaID,
                    SubscriberPhoto = photoPath, // Use the uploaded file path
                    IsActive = model.IsActive,
                    Address = model.Address,
                    City = model.City,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    Note = model.Note,
                    RegistrationId = model.RegistrationId,
                    CreatedDate = DateTime.Now
                };

                _context.Add(parent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Parent created: {ParentId}", parent.Id);

                // Add Phone Numbers if provided
                if (!string.IsNullOrEmpty(model.Phone1))
                {
                    var phone1 = new ParentPhone
                    {
                        PhoneNo = model.Phone1,
                        Label = model.Phone1Label ?? "Mobile",
                        IsWhatsApp = model.Phone1IsWhatsApp,
                        Notes = model.Phone1Notes,
                        IsPrimary = true,
                        ParentId = parent.Id,
                        CreatedDate = DateTime.Now
                    };
                    _context.Add(phone1);
                }

                if (!string.IsNullOrEmpty(model.Phone2))
                {
                    var phone2 = new ParentPhone
                    {
                        PhoneNo = model.Phone2,
                        Label = model.Phone2Label ?? "Home",
                        IsWhatsApp = model.Phone2IsWhatsApp,
                        Notes = model.Phone2Notes,
                        IsPrimary = false,
                        ParentId = parent.Id,
                        CreatedDate = DateTime.Now
                    };
                    _context.Add(phone2);
                }

                if (!string.IsNullOrEmpty(model.Phone3))
                {
                    var phone3 = new ParentPhone
                    {
                        PhoneNo = model.Phone3,
                        Label = model.Phone3Label ?? "Work",
                        IsWhatsApp = model.Phone3IsWhatsApp,
                        Notes = model.Phone3Notes,
                        IsPrimary = false,
                        ParentId = parent.Id,
                        CreatedDate = DateTime.Now
                    };
                    _context.Add(phone3);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Parent and phones created successfully");

                TempData["SuccessMessage"] = "Parent and phone numbers added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parent");
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            var studentsReload = await _context.RGManagements.OrderBy(r => r.Name).ToListAsync();
            ViewBag.Students = studentsReload;
            return View(model);
        }

        // GET: Parents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var parent = await _context.Parents
                .Include(p => p.Registration)
                .Include(p => p.PhoneNumbers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (parent == null)
                return NotFound();

            return View(parent);
        }

        // GET: Parents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var parent = await _context.Parents
                .Include(p => p.PhoneNumbers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parent == null)
                return NotFound();

            var phones = parent.PhoneNumbers.OrderBy(p => p.IsPrimary ? 0 : 1).ToList();

            // Use ParentCreateViewModel for editing
            var model = new ParentCreateViewModel
            {
                Fullname = parent.Fullname,
                Email = parent.Email,
                MobileNo = parent.MobileNo,
                Password = parent.Password,
                Gender = parent.Gender,
                IqamaID = parent.IqamaID,
                SubscriberPhoto = parent.SubscriberPhoto,
                IsActive = parent.IsActive,
                Address = parent.Address,
                City = parent.City,
                Latitude = parent.Latitude,
                Longitude = parent.Longitude,
                Note = parent.Note,
                RegistrationId = parent.RegistrationId
            };

            // Load phone numbers
            if (phones.Count > 0)
            {
                model.Phone1 = phones[0].PhoneNo;
                model.Phone1Label = phones[0].Label;
                model.Phone1IsWhatsApp = phones[0].IsWhatsApp;
                model.Phone1Notes = phones[0].Notes;
            }

            if (phones.Count > 1)
            {
                model.Phone2 = phones[1].PhoneNo;
                model.Phone2Label = phones[1].Label;
                model.Phone2IsWhatsApp = phones[1].IsWhatsApp;
                model.Phone2Notes = phones[1].Notes;
            }

            if (phones.Count > 2)
            {
                model.Phone3 = phones[2].PhoneNo;
                model.Phone3Label = phones[2].Label;
                model.Phone3IsWhatsApp = phones[2].IsWhatsApp;
                model.Phone3Notes = phones[2].Notes;
            }

            // Load students for dropdown
            var students = await _context.RGManagements.OrderBy(r => r.Name).ToListAsync();
            ViewBag.Students = students;

            // Pass the ID through ViewBag for the form
            ViewBag.ParentId = id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ParentCreateViewModel model, IFormFile SubscriberPhoto, string ExistingPhoto)
        {
            try
            {
                // Remove SubscriberPhoto from ModelState since it's optional for edit
                ModelState.Remove("SubscriberPhoto");

                var parent = await _context.Parents
                    .Include(p => p.PhoneNumbers)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (parent == null)
                {
                    TempData["ErrorMessage"] = "Parent not found.";
                    return NotFound();
                }

                // Preserve the existing photo path in model
                if (SubscriberPhoto == null || SubscriberPhoto.Length == 0)
                {
                    model.SubscriberPhoto = !string.IsNullOrEmpty(ExistingPhoto) ? ExistingPhoto : parent.SubscriberPhoto;
                }

                if (!ModelState.IsValid)
                {
                    var students = await _context.RGManagements.OrderBy(r => r.Name).ToListAsync();
                    ViewBag.Students = students;
                    ViewBag.ParentId = id;

                    // Log validation errors for debugging
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", errors));

                    return View(model);
                }

                // Handle file upload ONLY if a new file is provided
                if (SubscriberPhoto != null && SubscriberPhoto.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(SubscriberPhoto.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("SubscriberPhoto", "Only image files (jpg, jpeg, png, gif) are allowed.");
                        var students = await _context.RGManagements.OrderBy(r => r.Name).ToListAsync();
                        ViewBag.Students = students;
                        ViewBag.ParentId = id;
                        model.SubscriberPhoto = !string.IsNullOrEmpty(ExistingPhoto) ? ExistingPhoto : parent.SubscriberPhoto;
                        return View(model);
                    }

                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(parent.SubscriberPhoto))
                    {
                        var oldPhotoPath = Path.Combine(_webHostEnvironment.WebRootPath, parent.SubscriberPhoto.TrimStart('/'));
                        if (System.IO.File.Exists(oldPhotoPath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldPhotoPath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Could not delete old photo: {PhotoPath}", oldPhotoPath);
                            }
                        }
                    }

                    // Create unique filename
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "parents");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await SubscriberPhoto.CopyToAsync(fileStream);
                    }

                    // Update photo path
                    parent.SubscriberPhoto = $"/uploads/parents/{fileName}";
                    _logger.LogInformation("New photo uploaded for parent {ParentId}: {PhotoPath}", parent.Id, parent.SubscriberPhoto);
                }
                else
                {
                    // Keep existing photo
                    parent.SubscriberPhoto = !string.IsNullOrEmpty(ExistingPhoto) ? ExistingPhoto : parent.SubscriberPhoto;
                    _logger.LogInformation("Keeping existing photo for parent {ParentId}: {PhotoPath}", parent.Id, parent.SubscriberPhoto);
                }

                // Update parent details
                parent.Fullname = model.Fullname;
                parent.Email = model.Email;
                parent.MobileNo = model.MobileNo;
                parent.Password = model.Password;
                parent.Gender = model.Gender ?? "Male"; // Default to Male if null
                parent.IqamaID = model.IqamaID;
                parent.IsActive = model.IsActive;
                parent.Address = model.Address;
                parent.City = model.City;
                parent.Latitude = model.Latitude;
                parent.Longitude = model.Longitude;
                parent.Note = model.Note;
                parent.RegistrationId = model.RegistrationId;
                parent.LastUpdated = DateTime.Now;

                _context.Update(parent);

                // Handle Phone Numbers - Remove all existing and add new ones
                var existingPhones = parent.PhoneNumbers.ToList();
                _context.RemoveRange(existingPhones);

                // Add Phone 1
                if (!string.IsNullOrEmpty(model.Phone1))
                {
                    var phone1 = new ParentPhone
                    {
                        PhoneNo = model.Phone1,
                        Label = model.Phone1Label ?? "Mobile",
                        IsWhatsApp = model.Phone1IsWhatsApp,
                        Notes = model.Phone1Notes,
                        IsPrimary = true,
                        ParentId = parent.Id,
                        CreatedDate = DateTime.Now
                    };
                    _context.Add(phone1);
                }

                // Add Phone 2
                if (!string.IsNullOrEmpty(model.Phone2))
                {
                    var phone2 = new ParentPhone
                    {
                        PhoneNo = model.Phone2,
                        Label = model.Phone2Label ?? "Home",
                        IsWhatsApp = model.Phone2IsWhatsApp,
                        Notes = model.Phone2Notes,
                        IsPrimary = false,
                        ParentId = parent.Id,
                        CreatedDate = DateTime.Now
                    };
                    _context.Add(phone2);
                }

                // Add Phone 3
                if (!string.IsNullOrEmpty(model.Phone3))
                {
                    var phone3 = new ParentPhone
                    {
                        PhoneNo = model.Phone3,
                        Label = model.Phone3Label ?? "Work",
                        IsWhatsApp = model.Phone3IsWhatsApp,
                        Notes = model.Phone3Notes,
                        IsPrimary = false,
                        ParentId = parent.Id,
                        CreatedDate = DateTime.Now
                    };
                    _context.Add(phone3);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Parent updated successfully: {ParentId}", parent.Id);
                TempData["SuccessMessage"] = "Parent updated successfully!";

                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error updating parent {ParentId}", id);
                TempData["ErrorMessage"] = "Database error: " + dbEx.InnerException?.Message ?? dbEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating parent {ParentId}", id);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            // Reload data for view
            var studentsReload = await _context.RGManagements.OrderBy(r => r.Name).ToListAsync();
            ViewBag.Students = studentsReload;
            ViewBag.ParentId = id;

            // Ensure photo is preserved
            if (string.IsNullOrEmpty(model.SubscriberPhoto))
            {
                var parentReload = await _context.Parents.FindAsync(id);
                if (parentReload != null)
                {
                    model.SubscriberPhoto = parentReload.SubscriberPhoto;
                }
            }

            return View(model);
        }


        //GET: Parents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var parent = await _context.Parents
                .Include(p => p.Registration)
                .Include(p => p.PhoneNumbers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (parent == null)
                return NotFound();

            return View(parent);
        }

        // POST: Parents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var parent = await _context.Parents
                    .Include(p => p.PhoneNumbers)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (parent != null)
                {
                    // Delete photo file if exists
                    if (!string.IsNullOrEmpty(parent.SubscriberPhoto))
                    {
                        var photoPath = Path.Combine(_webHostEnvironment.WebRootPath, parent.SubscriberPhoto.TrimStart('/'));
                        if (System.IO.File.Exists(photoPath))
                        {
                            System.IO.File.Delete(photoPath);
                        }
                    }

                    _context.Parents.Remove(parent);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Parent deleted successfully!";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting parent");
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private bool ParentExists(int id)
        {
            return _context.Parents.Any(e => e.Id == id);
        }
    }
}