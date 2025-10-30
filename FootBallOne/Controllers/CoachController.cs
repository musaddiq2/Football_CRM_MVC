using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FootBallOne.Controllers
{
    public class CoachController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // To access wwwroot folder

        public CoachController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Coach
        // Lists all coaches, with optional search and filter functionality
        public async Task<IActionResult> Index(string searchString, string specializationFilter)
        {
            var coaches = from c in _context.Coaches
                          select c;

            // Search by FullName
            if (!string.IsNullOrEmpty(searchString))
            {
                coaches = coaches.Where(c => c.FullName.Contains(searchString) || c.Email.Contains(searchString));
            }

            // Filter by Specialization
            if (!string.IsNullOrEmpty(specializationFilter) && specializationFilter != "All")
            {
                coaches = coaches.Where(c => c.Specialization == specializationFilter);
            }

            // Populate specialization dropdown for the view
            ViewBag.Specializations = new SelectList(await _context.Coaches
                                                        .Select(c => c.Specialization)
                                                        .Distinct()
                                                        .ToListAsync());

            return View(await coaches.ToListAsync());
        }

        // GET: Coach/Details/5
        // Displays details of a specific coach
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coach = await _context.Coaches
                .FirstOrDefaultAsync(m => m.CoachId == id);
            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        // GET: Coach/Create
        // Displays the form to create a new coach
        public IActionResult Create()
        {
            return View();
        }

        // POST: Coach/Create
        // Handles the submission of the new coach form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CoachId,FullName,Age,PhoneNumber,Email,Specialization,ExperienceYears,WorkingHours,AssignedPlayers,JoiningDate,PhotoFile")] Coach coach)
        {
            if (ModelState.IsValid)
            {
                // Handle photo upload
                if (coach.PhotoFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(coach.PhotoFile.FileName);
                    string path = Path.Combine(wwwRootPath, "images", "coaches", fileName); // wwwroot/images/coaches

                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "images", "coaches"));

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await coach.PhotoFile.CopyToAsync(fileStream);
                    }
                    coach.PhotoURL = "/images/coaches/" + fileName; // Store relative URL
                }

                _context.Add(coach);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coach added successfully!";
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, re-render the form with validation errors
            return View(coach);
        }

        // GET: Coach/Edit/5
        // Displays the form to edit an existing coach
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coach = await _context.Coaches.FindAsync(id);
            if (coach == null)
            {
                return NotFound();
            }
            return View(coach);
        }

        // POST: Coach/Edit/5
        // Handles the submission of the edit coach form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CoachId,FullName,Age,PhoneNumber,Email,Specialization,ExperienceYears,WorkingHours,AssignedPlayers,JoiningDate,PhotoURL,PhotoFile")] Coach coach)
        {
            if (id != coach.CoachId)
            {
                return NotFound();
            }

            // Preserve the existing PhotoURL if no new file is uploaded
            // This is crucial because PhotoURL is not bound from the form directly unless it's a hidden field
            var existingCoach = await _context.Coaches.AsNoTracking().FirstOrDefaultAsync(c => c.CoachId == id);
            if (existingCoach == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle new photo upload
                    if (coach.PhotoFile != null)
                    {
                        // Delete old photo if it exists
                        if (!string.IsNullOrEmpty(existingCoach.PhotoURL))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingCoach.PhotoURL.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(coach.PhotoFile.FileName);
                        string path = Path.Combine(wwwRootPath, "images", "coaches", fileName);

                        Directory.CreateDirectory(Path.Combine(wwwRootPath, "images", "coaches"));

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await coach.PhotoFile.CopyToAsync(fileStream);
                        }
                        coach.PhotoURL = "/images/coaches/" + fileName;
                    }
                    else
                    {
                        // If no new photo is uploaded, retain the existing one
                        coach.PhotoURL = existingCoach.PhotoURL;
                    }

                    _context.Update(coach);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Coach updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoachExists(coach.CoachId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(coach);
        }

        // GET: Coach/Delete/5
        // Displays the confirmation page for deleting a coach
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coach = await _context.Coaches
                .FirstOrDefaultAsync(m => m.CoachId == id);
            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        // POST: Coach/Delete/5
        // Confirms and performs the deletion of a coach
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coach = await _context.Coaches.FindAsync(id);
            if (coach != null)
            {
                // Delete associated photo file
                if (!string.IsNullOrEmpty(coach.PhotoURL))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, coach.PhotoURL.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Coaches.Remove(coach);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coach deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CoachExists(int id)
        {
            return _context.Coaches.Any(e => e.CoachId == id);
        }

    }
}
