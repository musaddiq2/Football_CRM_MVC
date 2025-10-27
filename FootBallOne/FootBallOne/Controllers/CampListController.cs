using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FootBallOne.Controllers
{
    public class CampListController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CampListController> _logger;

        public CampListController(ApplicationDbContext context, ILogger<CampListController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: CampList
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var camps = await _context.Camps
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(camps);
        }

        // GET: CampList/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var camp = await _context.Camps.FirstOrDefaultAsync(c => c.CampID == id);
            if (camp == null)
            {
                return NotFound();
            }

            var activities = await _context.CampActivities
                .Where(a => a.CampID == id)
                .ToListAsync();

            var facilities = await _context.CampInvitedFacilities
                .Where(f => f.CampID == id)
                .ToListAsync();

            var model = new CampViewModel
            {
                CampName = camp.CampName,
                CampDescription = camp.CampDescription,
                StartDate = camp.StartDate,
                EndDate = camp.EndDate,
                Branch = camp.Branch,
                Capacity = camp.Capacity,
                TicketPrice = camp.TicketPrice,
                Address = camp.Address,
                City = camp.City,
                Latitude = camp.Latitude.HasValue ? (double?)Convert.ToDouble(camp.Latitude.Value) : null,
                Longitude = camp.Longitude.HasValue ? (double?)Convert.ToDouble(camp.Longitude.Value) : null,
                Activities = activities,
                InvitedFacilities = facilities,
                IsActive = camp.IsActive,
                CampID= camp.CampID
            };

            return View(model);
        }

        
     
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var camp = await _context.Camps.FindAsync(id);
            if (camp == null)
            {
                return NotFound();
            }

            var activities = await _context.CampActivities
                .Where(a => a.CampID == id)
                .ToListAsync();

            var facilities = await _context.CampInvitedFacilities
                .Where(f => f.CampID == id)
                .ToListAsync();


            var model = new CampViewModel
            {
                CampName = camp.CampName,
                CampDescription = camp.CampDescription,
                StartDate = camp.StartDate,
                EndDate = camp.EndDate,
                Branch = camp.Branch, 
                Capacity = camp.Capacity,
                TicketPrice = camp.TicketPrice,
                Address = camp.Address,
                City = camp.City,
                Latitude = camp.Latitude.HasValue ? (double?)Convert.ToDouble(camp.Latitude.Value) : null,
                Longitude = camp.Longitude.HasValue ? (double?)Convert.ToDouble(camp.Longitude.Value) : null,
                Activities = activities,
                IsActive = camp.IsActive,
                InvitedFacilities = facilities  
            };

            // ADD THIS: Prepare branch dropdown with selected value
            ViewBag.Branches = new SelectList(
                new[] {
            new { Value = "الفرع الرئيسي", Text = "الفرع الرئيسي" }
                },
                "Value",
                "Text",
                camp.Branch  // This sets the selected value
            );

            ViewBag.CampID = camp.CampID; // Pass ID for POST
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CampViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Activities = model.Activities ?? new List<CampActivity> { new CampActivity() };
                model.InvitedFacilities = model.InvitedFacilities ?? new List<CampInvitedFacility> { new CampInvitedFacility() };
                ViewBag.CampID = id;
                return View(model);
            }

            var camp = await _context.Camps.FindAsync(id);
            if (camp == null)
            {
                return NotFound();
            }

            // Update camp fields
            camp.CampName = model.CampName?.Trim();
            camp.CampDescription = model.CampDescription?.Trim();
            camp.StartDate = model.StartDate;
            camp.EndDate = model.EndDate;
            camp.Branch = model.Branch?.Trim();
            camp.Capacity = model.Capacity ?? 0;
            camp.TicketPrice = model.TicketPrice;
            camp.Address = model.Address?.Trim();
            camp.City = model.City?.Trim();
            camp.Latitude = model.Latitude.HasValue ? (decimal?)model.Latitude.Value : null;
            camp.Longitude = model.Longitude.HasValue ? (decimal?)model.Longitude.Value : null;
            camp.IsActive = model.IsActive;
            _context.Camps.Update(camp);

            // Remove old activities and facilities
            var oldActivities = _context.CampActivities.Where(a => a.CampID == id);
            var oldFacilities = _context.CampInvitedFacilities.Where(f => f.CampID == id);
            _context.CampActivities.RemoveRange(oldActivities);
            _context.CampInvitedFacilities.RemoveRange(oldFacilities);

            // Add new activities
            if (model.Activities != null)
            {
                foreach (var activity in model.Activities.Where(a => !string.IsNullOrWhiteSpace(a.ActivityName)))
                {
                    _context.CampActivities.Add(new CampActivity
                    {
                        CampID = id,
                        ActivityName = activity.ActivityName.Trim(),
                        ActivityDescription = activity.ActivityDescription?.Trim()
                    });
                }
            }

            // Add new facilities
            if (model.InvitedFacilities != null)
            {
                foreach (var facility in model.InvitedFacilities.Where(f => !string.IsNullOrWhiteSpace(f.FacilityName)))
                {
                    _context.CampInvitedFacilities.Add(new CampInvitedFacility
                    {
                        CampID = id,   
                        FacilityName = facility.FacilityName.Trim(),
                        InvitationMessage = facility.InvitationMessage?.Trim()
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Camp updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var camp = await _context.Camps.FindAsync(id);
            if (camp == null)
            {
                return NotFound();
            }

            return View(camp);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var camp = await _context.Camps.FindAsync(id);
            if (camp == null)
            {
                return NotFound();
            }

            // Remove related activities and facilities
            var activities = _context.CampActivities.Where(a => a.CampID == id);
            var facilities = _context.CampInvitedFacilities.Where(f => f.CampID == id);
            _context.CampActivities.RemoveRange(activities);
            _context.CampInvitedFacilities.RemoveRange(facilities);

            _context.Camps.Remove(camp);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Camp deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

    }
}
