using FootBallOne.Data;
using FootBallOne.Models;
using FootBallOne.Services;
using FootBallOne.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FootBallOne.Controllers
{
    public class EventListController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public EventListController(ApplicationDbContext context, IAcademyContext academyContext)
        : base(academyContext)
        {
            _context = context;
        }

        // GET: EventList
        public async Task<IActionResult> Index(string filterType = "Default", string searchTerm = "", int page = 1, int pageSize = 5)
        {
            var query = FilterByAcademy(_context.Events.AsQueryable()); // Show all events (active and inactive)

            // Apply filters based on filter type
            switch (filterType)
            {
                case "Upcoming":
                    query = query.Where(e => e.StartDate > DateTime.Now && e.IsActive);
                    break;
                case "Ongoing":
                    query = query.Where(e => e.StartDate <= DateTime.Now && e.EndDate >= DateTime.Now && e.IsActive);
                    break;
                case "Competition":
                    query = query.Where(e => (e.EventType == "Competition" || e.EventType == "Matches") && e.IsActive);
                    break;
                case "Available":
                    query = query.Where(e => e.AvailableCapacity > 0 && e.IsActive);
                    break;
                case "Active":
                    query = query.Where(e => e.IsActive);
                    break;
                case "Inactive":
                    query = query.Where(e => !e.IsActive);
                    break;
                case "Default":
                default:
                    // Show all events (both active and inactive)
                    break;
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Title.Contains(searchTerm) ||
                                        e.Description.Contains(searchTerm) ||
                                        e.City.Contains(searchTerm));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var eventsList = await query
                .OrderBy(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Convert to view model
            var events = eventsList.Select(e => new EventListViewModel
            {
                Id = e.Id,
                EventName = e.Title,
                DateInterval = e.StartDate.ToString("MMM d, yyyy") + " - " + e.EndDate.ToString("MMM d, yyyy"),
                AvailableCapacity = e.AvailableCapacity,
                AvailableSpots = e.AvailableCapacity, // You can calculate registered spots here
                TicketPrice = e.TicketPrice,
                EventType = e.EventType,
                IsActive = e.IsActive,
                Address = e.Adress + ", " + e.City,
                StartDate = e.StartDate,
                EndDate = e.EndDate
            }).ToList();

            var viewModel = new EventListFilterViewModel
            {
                Events = events,
                FilterType = filterType,
                SearchTerm = searchTerm,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        // POST: EventList/SellTicket
        [HttpPost]
        public async Task<IActionResult> SellTicket(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (eventItem.AvailableCapacity <= 0)
            {
                return Json(new { success = false, message = "No tickets available" });
            }

            // Decrease available capacity
            eventItem.AvailableCapacity--;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Ticket sold successfully", remainingCapacity = eventItem.AvailableCapacity });
        }

        // GET: EventList/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            var viewModel = new CreateEventViewModel
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                TicketPrice = eventItem.TicketPrice,
                StartDate = eventItem.StartDate,
                EndDate = eventItem.EndDate,
                Branch = eventItem.Branch,
                AvailableCapacity = eventItem.AvailableCapacity,
                EventType = eventItem.EventType,
                Adress = eventItem.Adress,
                City = eventItem.City,
                Latitude = (float)eventItem.Latitude,
                Longitude = (float)eventItem.Longitude,
                InvitedFacility = eventItem.InvitedFacility,
                InvitationMessage = eventItem.InvitationMessage,
                IsActive = eventItem.IsActive
            };

            // Prepare dropdown lists
            ViewBag.Branches = new SelectList(new[] { "الفرع الرئيسي", "branch" }, eventItem.Branch);
            ViewBag.EventTypes = new SelectList(
                new[] { "Festival", "WorkShop", "Seminar", "Conference", "Competition", "Training Course", "Trip", "Exhibition", "Other Event" },
                eventItem.EventType);

            return View(viewModel);
        }
        // POST: EventList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateEventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eventItem = await _context.Events.FindAsync(id);

                if (eventItem == null)
                {
                    return NotFound();
                }

                // Update event properties
                eventItem.Title = model.Title;
                eventItem.Description = model.Description;
                eventItem.TicketPrice = model.TicketPrice;
                eventItem.StartDate = model.StartDate;
                eventItem.EndDate = model.EndDate;
                eventItem.Branch = model.Branch;
                eventItem.AvailableCapacity = model.AvailableCapacity;
                eventItem.EventType = model.EventType;
                eventItem.Adress = model.Adress;
                eventItem.City = model.City;
                eventItem.Latitude = model.Latitude;
                eventItem.Longitude = model.Longitude;
                eventItem.InvitedFacility = model.InvitedFacility;
                eventItem.InvitationMessage = model.InvitationMessage;
                eventItem.IsActive = model.IsActive;

                try
                {
                    _context.Events.Update(eventItem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EventExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(model);
        }

        [HttpGet("Details/{id:int:min(0)}")]  // Attribute routing: Requires int id >=0, unique endpoint
        public async Task<IActionResult> Details(int id)
        {
            // Fetch the event from database
            var eventModel = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventModel == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map to ViewModel
            var viewModel = new CreateEventViewModel
            {
                Id = eventModel.Id,
                Title = eventModel.Title,
                Description = eventModel.Description,
                StartDate = eventModel.StartDate,
                EndDate = eventModel.EndDate,
                Branch = eventModel.Branch,
                AvailableCapacity = eventModel.AvailableCapacity,
                EventType = eventModel.EventType,
                Adress = eventModel.Adress,
                City = eventModel.City,
                Latitude = (float)eventModel.Latitude,
                Longitude = (float)eventModel.Longitude,
                TicketPrice = eventModel.TicketPrice,
                InvitedFacility = eventModel.InvitedFacility,
                InvitationMessage = eventModel.InvitationMessage
            };

            return View(viewModel);
        }

        // GET: EventList/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _context.Events
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // FIX: Map to CreateEventViewModel (matches Edit/Details)
            var viewModel = new CreateEventViewModel
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                TicketPrice = eventItem.TicketPrice,
                StartDate = eventItem.StartDate,
                EndDate = eventItem.EndDate,
                Branch = eventItem.Branch,
                AvailableCapacity = eventItem.AvailableCapacity,
                EventType = eventItem.EventType,
                Adress = eventItem.Adress,
                City = eventItem.City,
                Latitude = (float)eventItem.Latitude,
                Longitude = (float)eventItem.Longitude,
                InvitedFacility = eventItem.InvitedFacility,
                InvitationMessage = eventItem.InvitationMessage,
            };

            return View(viewModel); // Now passes correct ViewModel type
        }

        // POST: EventList/Delete/5
        [HttpPost]
        [ActionName("Delete")] // FIX: Correct ActionName to match form asp-action="Delete"
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            try
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EventExists(id))
                {
                    return NotFound();
                }
                // Optionally reload for user to retry
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private async Task<bool> EventExists(int id)
        {
            // FIX: Remove IsActive check for delete (allow deleting inactive events)
            return await _context.Events.AnyAsync(e => e.Id == id);
        }
    }
}