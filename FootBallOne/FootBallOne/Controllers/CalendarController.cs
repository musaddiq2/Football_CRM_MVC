using FootBallOne.Data;
using FootBallOne.Models;
using FootBallOne.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Added for logging
using Newtonsoft.Json;

namespace FootBallOne.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CalendarController> _logger; // Added for logging

        public CalendarController(ApplicationDbContext context, ILogger<CalendarController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? year, int? month, string view = "month")
        {
            var currentDate = new DateTime(year ?? DateTime.UtcNow.Year, month ?? DateTime.UtcNow.Month, 1);

            // Log the current date for debugging
            _logger.LogInformation("Calendar Index: CurrentDate = {CurrentDate}, View = {View}", currentDate, view);

            // Fetch events
            var events = await _context.Events
                .Where(e => e.IsActive)
                .Select(e => new CalendarItem
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsActive = e.IsActive,
                    IsCamp = false,
                    IsMatch = false,
                    Description = e.Description
                })
                .ToListAsync();

            // Fetch camps
            var camps = await _context.Camps
                .Where(c => c.IsActive)
                .Select(c => new CalendarItem
                {
                    Id = c.CampID,
                    Title = c.CampName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    IsCamp = true,
                    IsMatch = false,
                    Description = c.CampDescription
                })
                .ToListAsync();

            // Fetch matches (optimized to filter in database where possible)
            var matches = await _context.MatchInfo
                .Where(m => m.IsActive && m.MatchDate >= DateTime.UtcNow.Date)
                .Select(m => new CalendarItem
                {
                    Id = m.MatchId,
                    Title = $"{m.HomeTeam} vs {m.AwayTeam}",
                    StartDate = m.MatchDate.Add(m.MatchTime),
                    EndDate = m.MatchDate.Add(m.MatchTime).AddHours(2),
                    IsActive = m.IsActive,
                    IsCamp = false,
                    IsMatch = true,
                    Description = $"Tournament: {m.Tournament}, Venue: {m.Venue}, Status: {m.MatchStatus}",
                    HomeTeam = m.HomeTeam,
                    AwayTeam = m.AwayTeam,
                    Tournament = m.Tournament,
                    Venue = m.Venue,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore,
                    MatchStatus = m.MatchStatus,
                    Referee = m.Referee,
                    VIPGuests = m.VIPGuests,
                    Sponsors = m.Sponsors,
                    InvitationNotes = m.InvitationNotes
                })
                .ToListAsync();

            // Fetch trainings
            var trainings = await _context.Trainings
                .Where(t => t.IsActive)
                .Include(t => t.CourseSchedules)
                .Select(t => new CalendarItem
                {
                    Id = t.TrainingId,
                    Title = t.ActivityName,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsActive = t.IsActive,
                    IsCamp = false,
                    IsMatch = false,
                    IsTraining = true,
                    Description = $"{t.ActivityType} - {t.TrainingLevel ?? "All Levels"}" +
                                 (string.IsNullOrEmpty(t.Description) ? "" :
                                  t.Description.Length > 50 ? ": " + t.Description.Substring(0, 50) + "..." :
                                  ": " + t.Description),
                    TrainingSchedules = t.CourseSchedules.Select(cs => new ScheduleInfo
                    {
                        CourseScheduleId = cs.CourseScheduleId,
                        Day = cs.Day,
                        StartTime = cs.StartTime
                    }).ToList()
                })
                .ToListAsync();

            // Fetch tournaments (updated to include ongoing tournaments)
            var tournaments = await _context.Tournaments
                .Where(t => t.IsActive && t.EndDate >= DateTime.UtcNow.Date)
                .Select(t => new CalendarItem
                {
                    Id = t.TournamentID,
                    Title = t.TournamentName,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsActive = t.IsActive,
                    IsCamp = false,
                    IsMatch = false,
                    IsTraining = false,
                    IsTournament = true,
                    Description = t.TournamentDescription != null && t.TournamentDescription.Length > 50
                        ? t.TournamentDescription.Substring(0, 50) + "..."
                        : t.TournamentDescription ?? ""
                })
                .ToListAsync();

            // Log retrieved items for debugging
            _logger.LogInformation("Retrieved items - Events: {EventCount}, Camps: {CampCount}, Matches: {MatchCount}, Trainings: {TrainingCount}, Tournaments: {TournamentCount}",
                events.Count, camps.Count, matches.Count, trainings.Count, tournaments.Count);
            foreach (var t in tournaments)
            {
                _logger.LogInformation("Tournament: {Title}, Start: {Start}, End: {End}, IsActive: {IsActive}",
                    t.Title, t.StartDate, t.EndDate, t.IsActive);
            }

            // Combine all items
            var combinedItems = events.Concat(camps).Concat(matches).Concat(trainings).Concat(tournaments).ToList();

            var viewModel = new CalendarViewModel
            {
                CurrentDate = currentDate,
                Items = combinedItems,
                View = view
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            var viewModel = new CreateEventViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CreateEventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eventItem = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    TicketPrice = model.TicketPrice,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Branch = model.Branch,
                    AvailableCapacity = model.AvailableCapacity,
                    EventType = "Events",
                    Adress = model.Adress,
                    City = model.City,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    InvitedFacility = model.InvitedFacility,
                    InvitationMessage = model.InvitationMessage,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new
                {
                    year = model.StartDate.Year,
                    month = model.StartDate.Month
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                eventItem.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(DateTime start, DateTime end, string[] eventTypes = null)
        {
            var eventsQuery = _context.Events.Where(e => e.IsActive);

            if (eventTypes != null && eventTypes.Length > 0)
            {
                eventsQuery = eventsQuery.Where(e => eventTypes.Contains(e.EventType));
            }

            var events = await eventsQuery
                .Where(e => e.StartDate <= end && e.EndDate >= start)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = e.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    type = e.EventType,
                    description = e.Description
                })
                .ToListAsync();

            return Json(events);
        }

        [HttpGet]
        public IActionResult Navigate(int year, int month, string action)
        {
            ViewBag.DebugInfo = $"Navigate: {action} from {year}/{month}";

            System.Diagnostics.Debug.WriteLine($"Navigate called: year={year}, month={month}, action={action}");

            var currentDate = new DateTime(year, month, 1);

            switch (action?.ToLower())
            {
                case "prev":
                    currentDate = currentDate.AddMonths(-1);
                    break;
                case "next":
                    currentDate = currentDate.AddMonths(1);
                    break;
                case "today":
                    currentDate = DateTime.Today;
                    break;
            }

            var queryParams = new Dictionary<string, object>
            {
                ["year"] = currentDate.Year,
                ["month"] = currentDate.Month
            };

            if (Request.Query.ContainsKey("view"))
            {
                queryParams["view"] = Request.Query["view"].ToString();
            }

            var eventTypeKeys = Request.Query.Keys.Where(k => k.StartsWith("eventTypes")).ToList();
            foreach (var key in eventTypeKeys)
            {
                queryParams[key] = Request.Query[key].ToString();
            }

            System.Diagnostics.Debug.WriteLine($"Redirecting to: year={currentDate.Year}, month={currentDate.Month}");

            return RedirectToAction("Index", queryParams);
        }

        [HttpPost]
        public IActionResult NavigateAjax([FromBody] NavigateRequest request)
        {
            var currentDate = new DateTime(request.Year, request.Month, 1);

            switch (request.Action?.ToLower())
            {
                case "prev":
                    currentDate = currentDate.AddMonths(-1);
                    break;
                case "next":
                    currentDate = currentDate.AddMonths(1);
                    break;
                case "today":
                    currentDate = DateTime.Today;
                    break;
            }

            return Json(new
            {
                success = true,
                year = currentDate.Year,
                month = currentDate.Month
            });
        }

        [HttpGet]
        public IActionResult CreateCamp()
        {
            var model = new CampViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Activities = new List<CampActivity> { new CampActivity() },
                InvitedFacilities = new List<CampInvitedFacility> { new CampInvitedFacility() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCamp(CampViewModel model, string createAnother = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.CampName))
                    ModelState.AddModelError("CampName", "Camp name is required");
                if (model.StartDate == default)
                    ModelState.AddModelError("StartDate", "Start date is required");
                if (model.EndDate == default)
                    ModelState.AddModelError("EndDate", "End date is required");
                if (model.StartDate >= model.EndDate)
                    ModelState.AddModelError("EndDate", "End date must be after start date");
                if (string.IsNullOrWhiteSpace(model.Branch))
                    ModelState.AddModelError("Branch", "Branch is required");
                if (model.TicketPrice <= 0)
                    ModelState.AddModelError("TicketPrice", "Ticket price must be greater than 0");
                if (model.InvitedFacilities == null || !model.InvitedFacilities.Any(f => !string.IsNullOrWhiteSpace(f.FacilityName)))
                    ModelState.AddModelError("", "At least one invited facility is required");

                if (!ModelState.IsValid)
                {
                    model.Activities = model.Activities ?? new List<CampActivity> { new CampActivity() };
                    model.InvitedFacilities = model.InvitedFacilities ?? new List<CampInvitedFacility> { new CampInvitedFacility() };
                    return View(model);
                }

                var camp = new Camp
                {
                    CampName = model.CampName?.Trim(),
                    CampDescription = model.CampDescription?.Trim(),
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Branch = model.Branch?.Trim(),
                    Capacity = model.Capacity ?? 0,
                    TicketPrice = model.TicketPrice,
                    Address = model.Address?.Trim(),
                    City = model.City?.Trim(),
                    Latitude = model.Latitude.HasValue ? (decimal?)model.Latitude.Value : null,
                    Longitude = model.Longitude.HasValue ? (decimal?)model.Longitude.Value : null,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };


                _context.Camps.Add(camp);
                await _context.SaveChangesAsync();

                int campId = camp.CampID;

                if (model.Activities != null && model.Activities.Any())
                {
                    var validActivities = model.Activities
                        .Where(a => !string.IsNullOrWhiteSpace(a.ActivityName))
                        .ToList();

                    foreach (var activity in validActivities)
                    {
                        var campActivity = new CampActivity
                        {
                            CampID = campId,
                            ActivityName = activity.ActivityName.Trim(),
                            ActivityDescription = activity.ActivityDescription?.Trim()
                        };
                        _context.CampActivities.Add(campActivity);
                    }
                }

                if (model.InvitedFacilities != null && model.InvitedFacilities.Any())
                {
                    var validFacilities = model.InvitedFacilities
                        .Where(f => !string.IsNullOrWhiteSpace(f.FacilityName))
                        .ToList();

                    foreach (var facility in validFacilities)
                    {
                        var campFacility = new CampInvitedFacility
                        {
                            CampID = campId,
                            FacilityName = facility.FacilityName.Trim(),
                            InvitationMessage = facility.InvitationMessage?.Trim()
                        };
                        _context.CampInvitedFacilities.Add(campFacility);
                    }
                }

                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(createAnother) && createAnother.ToLower() == "true")
                {
                    TempData["SuccessMessage"] = "Camp created successfully! Create another one.";
                    return RedirectToAction("CreateCamp");
                }

                TempData["SuccessMessage"] = "Camp created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error creating camp: {Message}, Inner: {InnerException}", ex.Message, innerException);
                ModelState.AddModelError("", $"Error creating camp: {ex.Message}. Inner: {innerException}");
                model.Activities = model.Activities ?? new List<CampActivity> { new CampActivity() };
                model.InvitedFacilities = model.InvitedFacilities ?? new List<CampInvitedFacility> { new CampInvitedFacility() };
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult CreateMatch()
        {
            var model = new MatchViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMatch(MatchViewModel model, string createAnother = "")
        {
            try
            {
                // Enhanced validation
                if (string.IsNullOrWhiteSpace(model.HomeTeam))
                    ModelState.AddModelError("HomeTeam", "Home team is required");
                if (string.IsNullOrWhiteSpace(model.AwayTeam))
                    ModelState.AddModelError("AwayTeam", "Away team is required");
                if (model.MatchDate == default)
                    ModelState.AddModelError("MatchDate", "Match date is required");
                if (model.MatchTime == default)
                    ModelState.AddModelError("MatchTime", "Match time is required");
                if (string.IsNullOrWhiteSpace(model.MatchStatus))
                    ModelState.AddModelError("MatchStatus", "Match status is required");
                if (model.Tournament?.Length > 100)
                    ModelState.AddModelError("Tournament", "Tournament cannot exceed 100 characters");
                if (model.Venue?.Length > 500)
                    ModelState.AddModelError("Venue", "Venue cannot exceed 500 characters");
                if (model.Referee?.Length > 100)
                    ModelState.AddModelError("Referee", "Referee cannot exceed 100 characters");
                if (model.Winner?.Length > 100)
                    ModelState.AddModelError("Winner", "Winner cannot exceed 100 characters");
                if (model.VIPGuests?.Length > 250)
                    ModelState.AddModelError("VIPGuests", "VIP guests cannot exceed 250 characters");
                if (model.Sponsors?.Length > 250)
                    ModelState.AddModelError("Sponsors", "Sponsors cannot exceed 250 characters");
                if (model.InvitationNotes?.Length > 500)
                    ModelState.AddModelError("InvitationNotes", "Invitation notes cannot exceed 500 characters");
                if (model.HomeScore < 0)
                    ModelState.AddModelError("HomeScore", "Home score cannot be negative");
                if (model.AwayScore < 0)
                    ModelState.AddModelError("AwayScore", "Away score cannot be negative");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState invalid: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return View(model);
                }

                var match = new MatchInfo
                {
                    Tournament = model.Tournament?.Trim(),
                    HomeTeam = model.HomeTeam.Trim(),
                    AwayTeam = model.AwayTeam.Trim(),
                    MatchDate = model.MatchDate,
                    MatchTime = model.MatchTime,
                    Venue = model.Venue?.Trim(),
                    Referee = model.Referee?.Trim(),
                    HomeScore = model.HomeScore, // Non-nullable, defaults to 0
                    AwayScore = model.AwayScore, // Non-nullable, defaults to 0
                    Winner = model.Winner?.Trim(),
                    MatchStatus = model.MatchStatus.Trim(),
                    VIPGuests = model.VIPGuests?.Trim(),
                    Sponsors = model.Sponsors?.Trim(),
                    InvitationNotes = model.InvitationNotes?.Trim(),
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _logger.LogInformation("Attempting to save match: {@Match}", match);
                _context.MatchInfo.Add(match);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(createAnother) && createAnother.ToLower() == "true")
                {
                    TempData["SuccessMessage"] = "Match created successfully! Create another one.";
                    return RedirectToAction("CreateMatch");
                }

                TempData["SuccessMessage"] = "Match created successfully!";
                return RedirectToAction("Index", new { year = model.MatchDate.Year, month = model.MatchDate.Month });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error creating match: {Message}, Inner: {InnerException}", ex.Message, innerException);
                ModelState.AddModelError("", $"Error creating match: {ex.Message}. Inner: {innerException}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating match: {Message}", ex.Message);
                ModelState.AddModelError("", $"Error creating match: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            try
            {
                var match = await _context.MatchInfo.FindAsync(id);
                if (match == null)
                {
                    TempData["ErrorMessage"] = "Match not found.";
                    return RedirectToAction(nameof(Index));
                }

                match.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Match deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting match: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Error deleting match: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action called with null ID.");
                return NotFound();
            }

            var match = await _context.MatchInfo
                .FirstOrDefaultAsync(m => m.MatchId == id && m.IsActive);

            if (match == null)
            {
                _logger.LogWarning("Match with ID {Id} not found or is not active.", id);
                return NotFound();
            }

            var model = new MatchViewModel
            {
                MatchId = match.MatchId,
                Tournament = match.Tournament,
                HomeTeam = match.HomeTeam,
                AwayTeam = match.AwayTeam,
                MatchDate = match.MatchDate,
                MatchTime = match.MatchTime,
                Venue = match.Venue,
                Referee = match.Referee,
                HomeScore = match.HomeScore,
                AwayScore = match.AwayScore,
                Winner = match.Winner,
                MatchStatus = match.MatchStatus,
                VIPGuests = match.VIPGuests,
                Sponsors = match.Sponsors,
                InvitationNotes = match.InvitationNotes
            };

            return View(model);
        }

        // GET: Calendar/CreateTraining
        [HttpGet]
        public IActionResult CreateTraining()
        {
            var model = new TrainingViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                CourseSchedules = new List<CourseScheduleViewModel> { new CourseScheduleViewModel() }
            };
            return View(model);
        }

        // POST: Calendar/CreateTraining
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTraining(TrainingViewModel model, string createAnother = "")
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(model.ActivityName))
                    ModelState.AddModelError("ActivityName", "Activity name is required");
                if (string.IsNullOrWhiteSpace(model.ActivityType))
                    ModelState.AddModelError("ActivityType", "Activity type is required");
                if (model.StartDate == default)
                    ModelState.AddModelError("StartDate", "Start date is required");
                if (model.EndDate == default)
                    ModelState.AddModelError("EndDate", "End date is required");
                if (model.StartDate >= model.EndDate)
                    ModelState.AddModelError("EndDate", "End date must be after start date");
                if (model.MaximumCapacity < 0)
                    ModelState.AddModelError("MaximumCapacity", "Maximum capacity cannot be negative");
                if (model.MinimumSubscribers < 0)
                    ModelState.AddModelError("MinimumSubscribers", "Minimum subscribers cannot be negative");
                if (model.TotalCourseCost < 0)
                    ModelState.AddModelError("TotalCourseCost", "Total course cost cannot be negative");
                if (model.ProfitMargin < 0)
                    ModelState.AddModelError("ProfitMargin", "Profit margin cannot be negative");
                if (model.CourseSchedules == null || !model.CourseSchedules.Any(cs => !string.IsNullOrWhiteSpace(cs.Day)))
                    ModelState.AddModelError("", "At least one valid schedule is required");

                if (!ModelState.IsValid)
                {
                    model.CourseSchedules = model.CourseSchedules ?? new List<CourseScheduleViewModel> { new CourseScheduleViewModel() };
                    _logger.LogWarning("ModelState invalid: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return View(model);
                }

                var training = new Training
                {
                    ActivityImage = model.ActivityImage?.Trim(),
                    ActivityName = model.ActivityName.Trim(),
                    ActivityType = model.ActivityType.Trim(),
                    Description = model.Description?.Trim(),
                    TrainingLevel = model.TrainingLevel?.Trim(),
                    AgeGroup = model.AgeGroup?.Trim(),
                    Branch = model.Branch?.Trim(),
                    Gender = model.Gender?.Trim(),
                    Facilities = model.Facilities?.Trim(),
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,


                    MaximumCapacity = model.MaximumCapacity,
                    MinimumSubscribers = model.MinimumSubscribers,
                    CostType = model.CostType?.Trim(),
                    TotalCourseCost = model.TotalCourseCost,
                    ProfitMargin = model.ProfitMargin,
                    TermsConditions = model.TermsConditions?.Trim(),
                    Trainers = model.Trainers?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Trainings.Add(training);
                await _context.SaveChangesAsync();

                int trainingId = training.TrainingId;

                if (model.CourseSchedules != null && model.CourseSchedules.Any())
                {
                    var validSchedules = model.CourseSchedules
                        .Where(cs => !string.IsNullOrWhiteSpace(cs.Day))
                        .ToList();

                    foreach (var schedule in validSchedules)
                    {
                        var courseSchedule = new CourseSchedule
                        {
                            TrainingId = trainingId,
                            Day = schedule.Day.Trim(),
                            StartTime = schedule.StartTime
                        };
                        _context.CourseSchedules.Add(courseSchedule);
                    }
                }

                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(createAnother) && createAnother.ToLower() == "true")
                {
                    TempData["SuccessMessage"] = "Training created successfully! Create another one.";
                    return RedirectToAction("CreateTraining");
                }

                TempData["SuccessMessage"] = "Training created successfully!";
                return RedirectToAction("Index", new { year = model.StartDate.Year, month = model.StartDate.Month });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error creating training: {Message}, Inner: {InnerException}", ex.Message, innerException);
                ModelState.AddModelError("", $"Error creating training: {ex.Message}. Inner: {innerException}");
                model.CourseSchedules = model.CourseSchedules ?? new List<CourseScheduleViewModel> { new CourseScheduleViewModel() };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating training: {Message}", ex.Message);
                ModelState.AddModelError("", $"Error creating training: {ex.Message}");
                model.CourseSchedules = model.CourseSchedules ?? new List<CourseScheduleViewModel> { new CourseScheduleViewModel() };
                return View(model);
            }
        }

        // POST: Calendar/DeleteTraining
        [HttpPost]
        public async Task<IActionResult> DeleteTraining(int id)
        {
            try
            {
                var training = await _context.Trainings.FindAsync(id);
                if (training == null)
                {
                    TempData["ErrorMessage"] = "Training not found.";
                    return RedirectToAction(nameof(Index));
                }

                training.IsActive = false; // Soft delete to match your pattern
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Training deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting training: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Error deleting training: {ex.Message}";

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Calendar/TrainingDetails
        [HttpGet]
        public async Task<IActionResult> TrainingDetails(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("TrainingDetails action called with null ID.");
                return NotFound();
            }

            var training = await _context.Trainings
                .Include(t => t.CourseSchedules)
                .FirstOrDefaultAsync(t => t.TrainingId == id && t.IsActive);

            if (training == null)
            {
                _logger.LogWarning("Training with ID {Id} not found or is not active.", id);
                return NotFound();
            }

            var model = new TrainingViewModel
            {
                TrainingId = training.TrainingId,
                ActivityImage = training.ActivityImage,
                ActivityName = training.ActivityName,
                ActivityType = training.ActivityType,
                Description = training.Description,
                TrainingLevel = training.TrainingLevel,
                AgeGroup = training.AgeGroup,
                Branch = training.Branch,
                Gender = training.Gender,
                Facilities = training.Facilities,
                StartDate = training.StartDate,
                EndDate = training.EndDate,


                MaximumCapacity = training.MaximumCapacity,
                MinimumSubscribers = training.MinimumSubscribers,
                CostType = training.CostType,
                TotalCourseCost = training.TotalCourseCost,
                ProfitMargin = training.ProfitMargin,
                TermsConditions = training.TermsConditions,
                Trainers = training.Trainers,
                CourseSchedules = training.CourseSchedules.Select(cs => new CourseScheduleViewModel
                {
                    CourseScheduleId = cs.CourseScheduleId,
                    Day = cs.Day,
                    StartTime = cs.StartTime
                }).ToList()
            };

            return View(model);
        }

        // GET: Tournaments/CreateTournament
        [HttpGet]
        public IActionResult CreateTournament()
        {
            return View(new TournamentViewModel());
        }

        // POST: Tournaments/CreateTournament
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTournament(TournamentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var tournament = new Tournament
                    {
                        TournamentName = model.TournamentName,
                        TournamentDescription = model.TournamentDescription,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        Status = model.Status,
                        OpenToEveryone = model.OpenToEveryone,
                        Branch = model.Branch, // Add this line
                        IsActive = true,
                        CreateDate = DateTime.UtcNow
                    };

                    _context.Tournaments.Add(tournament);
                    await _context.SaveChangesAsync();

                    if (model.InvitedFacilities != null && model.InvitedFacilities.Any())
                    {
                        foreach (var facility in model.InvitedFacilities)
                        {
                            if (!string.IsNullOrEmpty(facility.Invitedfacility))
                            {
                                var invitedFacility = new InvitedFacility
                                {
                                    TournamentID = tournament.TournamentID,
                                    Invitedfacility = facility.Invitedfacility,
                                    InvitationMessage = facility.InvitationMessage
                                };
                                _context.InvitedFacilities.Add(invitedFacility);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    //var insertedTournament = await _context.Tournaments
                    //    .Include(t => t.InvitedFacilities)
                    //    .FirstOrDefaultAsync(t => t.TournamentID == tournament.TournamentID);
                    //if (insertedTournament != null && insertedTournament.InvitedFacilities.Any())
                    //{
                    //    TempData["Message"] = "Tournament and invited facilities created successfully!";
                    //}
                    //else
                    //{
                    //    TempData["Message"] = "Tournament created, but no invited facilities were added.";
                    //}

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error creating tournament: {Message}", ex.Message);
                    ModelState.AddModelError("", $"Error creating tournament: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error creating tournament: {Message}", ex.Message);
                    ModelState.AddModelError("", $"Error creating tournament: {ex.Message}");
                }
            }
            return View(model);
        }

        // GET: Tournaments/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action called with null ID.");
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .Include(t => t.InvitedFacilities)
                .FirstOrDefaultAsync(m => m.TournamentID == id);

            if (tournament == null)
            {
                _logger.LogWarning("Tournament with ID {Id} not found.", id);
                return NotFound();
            }

            var viewModel = new TournamentViewModel
            {
                TournamentName = tournament.TournamentName,
                TournamentDescription = tournament.TournamentDescription,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Status = tournament.Status,
                OpenToEveryone = tournament.OpenToEveryone,
                InvitedFacilities = tournament.InvitedFacilities.Select(f => new InvitedFacilityViewModel
                {
                    Invitedfacility = f.Invitedfacility,
                    InvitationMessage = f.InvitationMessage
                }).ToList()
            };

            return View(viewModel);
        }

        // PUT: Tournaments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TournamentViewModel model)
        {
            if (id != id) // Should be corrected to check against model.TournamentID if added
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tournament = await _context.Tournaments
                        .Include(t => t.InvitedFacilities)
                        .FirstOrDefaultAsync(m => m.TournamentID == id);

                    if (tournament == null)
                    {
                        return NotFound();
                    }

                    tournament.TournamentName = model.TournamentName;
                    tournament.TournamentDescription = model.TournamentDescription;
                    tournament.StartDate = model.StartDate;
                    tournament.EndDate = model.EndDate;
                    tournament.Status = model.Status;
                    tournament.OpenToEveryone = model.OpenToEveryone;

                    _context.InvitedFacilities.RemoveRange(tournament.InvitedFacilities);
                    if (model.InvitedFacilities != null && model.InvitedFacilities.Any())
                    {
                        foreach (var facility in model.InvitedFacilities)
                        {
                            if (!string.IsNullOrEmpty(facility.Invitedfacility))
                            {
                                var invitedFacility = new InvitedFacility
                                {
                                    TournamentID = tournament.TournamentID,
                                    Invitedfacility = facility.Invitedfacility,
                                    InvitationMessage = facility.InvitationMessage
                                };
                                _context.InvitedFacilities.Add(invitedFacility);
                            }
                        }
                    }

                    _context.Update(tournament);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Tournament updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error updating tournament: {Message}", ex.Message);
                    ModelState.AddModelError("", $"Error updating tournament: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error updating tournament: {Message}", ex.Message);
                    ModelState.AddModelError("", $"Error updating tournament: {ex.Message}");
                }
            }
            return View(model);
        }

        // DELETE: Tournaments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete1(int id)
        {
            try
            {
                var tournament = await _context.Tournaments
                    .Include(t => t.InvitedFacilities)
                    .FirstOrDefaultAsync(m => m.TournamentID == id);

                if (tournament == null)
                {
                    _logger.LogWarning("Tournament with ID {Id} not found for deletion.", id);
                    return NotFound();
                }

                _context.InvitedFacilities.RemoveRange(tournament.InvitedFacilities);
                _context.Tournaments.Remove(tournament);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Tournament deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting tournament: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Error deleting tournament: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting tournament: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Error deleting tournament: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    
}

    public class NavigateRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Action { get; set; }
        public string View { get; set; }
        public string[] EventTypes { get; set; }
    }
}