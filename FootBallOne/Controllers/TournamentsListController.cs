using FootBallOne.Data;
using FootBallOne.Models;
using FootBallOne.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FootBallOne.Controllers
{
    public class TournamentsListController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TournamentsListController> _logger;

        public TournamentsListController(ApplicationDbContext context, ILogger<TournamentsListController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Main Index - supports both tabs via AJAX, but initial tab is set by 'tab' query param
        public async Task<IActionResult> Index(string searchTerm = "", string tournament = "", string status = "", string tab = "tournaments")
        {
            // We only need to prepare MatchListViewModel if tab is "matches", but for simplicity,
            // we always prepare it since it's lightweight. Tournaments tab doesn't use it.
            var viewModel = new MatchListViewModel
            {
                SearchTerm = searchTerm,
                SelectedTournament = tournament,
                SelectedStatus = status
            };

            // Load matches data (used only in Matches tab)
            var matchesQuery = _context.MatchInfo.Where(m => m.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                matchesQuery = matchesQuery.Where(m =>
                    m.HomeTeam.Contains(searchTerm) ||
                    m.AwayTeam.Contains(searchTerm) ||
                    m.Tournament.Contains(searchTerm) ||
                    m.Venue.Contains(searchTerm) ||
                    m.Referee.Contains(searchTerm));
            }
            if (!string.IsNullOrWhiteSpace(tournament))
                matchesQuery = matchesQuery.Where(m => m.Tournament == tournament);
            if (!string.IsNullOrWhiteSpace(status))
                matchesQuery = matchesQuery.Where(m => m.MatchStatus == status);

            var matches = await matchesQuery
                .OrderByDescending(m => m.MatchDate)
                .ThenByDescending(m => m.MatchTime)
                .ToListAsync();

            viewModel.Matches = matches.Select(m => new MatchViewModel
            {
                MatchId = m.MatchId,
                Tournament = m.Tournament,
                HomeTeam = m.HomeTeam,
                AwayTeam = m.AwayTeam,
                MatchDate = m.MatchDate,
                MatchTime = m.MatchTime,
                Venue = m.Venue,
                Referee = m.Referee,
                HomeScore = m.HomeScore,
                AwayScore = m.AwayScore,
                Winner = m.Winner,
                MatchStatus = m.MatchStatus,
                VIPGuests = m.VIPGuests,
                Sponsors = m.Sponsors,
                InvitationNotes = m.InvitationNotes
            }).ToList();

            viewModel.Tournaments = await _context.MatchInfo
                .Where(m => m.IsActive && !string.IsNullOrEmpty(m.Tournament))
                .Select(m => m.Tournament)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            ViewBag.InitialTab = tab;
            return View(viewModel);
        }

        // === MATCHES TAB ===
        [HttpGet]
        public async Task<IActionResult> GetMatchesPartial(string searchTerm = "", string tournament = "", string status = "")
        {
            var viewModel = new MatchListViewModel
            {
                SearchTerm = searchTerm,
                SelectedTournament = tournament,
                SelectedStatus = status
            };

            var matchesQuery = _context.MatchInfo.Where(m => m.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                matchesQuery = matchesQuery.Where(m =>
                    m.HomeTeam.Contains(searchTerm) ||
                    m.AwayTeam.Contains(searchTerm) ||
                    m.Tournament.Contains(searchTerm) ||
                    m.Venue.Contains(searchTerm) ||
                    m.Referee.Contains(searchTerm));
            }
            if (!string.IsNullOrWhiteSpace(tournament))
                matchesQuery = matchesQuery.Where(m => m.Tournament == tournament);
            if (!string.IsNullOrWhiteSpace(status))
                matchesQuery = matchesQuery.Where(m => m.MatchStatus == status);

            var matches = await matchesQuery
                .OrderByDescending(m => m.MatchDate)
                .ThenByDescending(m => m.MatchTime)
                .ToListAsync();

            viewModel.Matches = matches.Select(m => new MatchViewModel
            {
                MatchId = m.MatchId,
                Tournament = m.Tournament,
                HomeTeam = m.HomeTeam,
                AwayTeam = m.AwayTeam,
                MatchDate = m.MatchDate,
                MatchTime = m.MatchTime,
                Venue = m.Venue,
                Referee = m.Referee,
                HomeScore = m.HomeScore,
                AwayScore = m.AwayScore,
                Winner = m.Winner,
                MatchStatus = m.MatchStatus,
                VIPGuests = m.VIPGuests,
                Sponsors = m.Sponsors,
                InvitationNotes = m.InvitationNotes
            }).ToList();

            return PartialView("_MatchesListPartial", viewModel);
        }

        // === TOURNAMENTS TAB ===
        [HttpGet]
        public async Task<IActionResult> GetTournamentsPartial(string searchTermTournament = "")
        {
            var tournaments = await _context.Tournaments
                .Include(t => t.InvitedFacilities)
                .Where(t => string.IsNullOrEmpty(searchTermTournament) || t.TournamentName.Contains(searchTermTournament))
                .OrderByDescending(t => t.StartDate)
                .Select(t => new TournamentsListView
                {
                    TournamentID = t.TournamentID,
                    TournamentName = t.TournamentName,
                    Branch = t.Branch,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Status = t.Status,
                    IsActive = t.IsActive,
                    InvitedFacilities = t.InvitedFacilities
                        .Select(f => f.Invitedfacility)
                        .ToList()
                })
                .ToListAsync();

            return PartialView("_TournamentsListPartial", tournaments);
        }

        // === TEAMS & INVITATIONS (PLACEHOLDERS) ===
        [HttpGet]
        public async Task<IActionResult> GetTeamsPartial(string searchTermTeam = "")
        {
            var teamsQuery = _context.Teams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTermTeam))
            {
                teamsQuery = teamsQuery.Where(t =>
                    t.TeamName.Contains(searchTermTeam) ||
                    t.Branch.Contains(searchTermTeam) ||
                    t.Stadium.Contains(searchTermTeam) ||
                    t.Coach.Contains(searchTermTeam));
            }

            var teams = await teamsQuery
                .OrderBy(t => t.TeamName)
                .Select(t => new TeamViewModel
                {
                    TeamID = t.TeamID,
                    TeamName = t.TeamName,
                    TrainingActivity = t.TrainingActivity,
                    Branch = t.Branch,
                    TeamLogoPath = t.TeamLogoPath,
                    Stadium = t.Stadium,
                    Coach = t.Coach,
                    Status = t.Status,
                    FoundationDate = t.FoundationDate,
                    TeamDescription = t.TeamDescription,
                    IsActive = t.IsActive
                })
                .ToListAsync();

            return PartialView("_TeamsListPartial", teams);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetInvitationsPartial(string searchTermInvitation = "")
        //{
        //    var invitations = new List<InvitationViewModel>();
        //    return PartialView("_InvitationsListPartial", invitations);
        //}

        //=== MATCH DETAILS(from Matches tab) ===
        public async Task<IActionResult> Details(int id, string? tab = "matches")
        {
            var match = await _context.MatchInfo.FindAsync(id);
            if (match == null) return NotFound();

            var viewModel = new MatchViewModel
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

            ViewBag.ReturnTab = tab; // Pass tab for "Back to List" link
            return View("Details", viewModel);
        }

        // === MATCH EDIT (from Matches tab) ===
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? tab = "matches", int step = 1)
        {
            var match = await _context.MatchInfo.FindAsync(id);
            if (match == null) return NotFound();

            var viewModel = new MatchViewModel
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

            ViewBag.ReturnTab = tab;
            ViewBag.CurrentStep = step;
            return View("Edit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MatchViewModel viewModel, string? tab = "matches", int currentStep = 1)
        {
            if (id != viewModel.MatchId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var match = await _context.MatchInfo.FindAsync(id);
                    if (match == null) return NotFound();

                    match.Tournament = viewModel.Tournament;
                    match.HomeTeam = viewModel.HomeTeam;
                    match.AwayTeam = viewModel.AwayTeam;
                    match.MatchDate = viewModel.MatchDate;
                    match.MatchTime = viewModel.MatchTime;
                    match.Venue = viewModel.Venue;
                    match.Referee = viewModel.Referee;
                    match.HomeScore = viewModel.HomeScore;
                    match.AwayScore = viewModel.AwayScore;
                    match.Winner = viewModel.Winner;
                    match.MatchStatus = viewModel.MatchStatus;
                    match.VIPGuests = viewModel.VIPGuests;
                    match.Sponsors = viewModel.Sponsors;
                    match.InvitationNotes = viewModel.InvitationNotes;

                    _context.Update(match);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Match updated successfully!";

                    // Redirect to Index page (match list) after successful save
                    return RedirectToAction("Index", new { tab = tab });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await MatchExists(id)) return NotFound();
                    else throw;
                }
            }

            // If validation fails, stay on the Edit page at the current step
            ViewBag.ReturnTab = tab;
            ViewBag.CurrentStep = currentStep;
            return View("Edit", viewModel);
        }

        // === MATCH DELETE ===
        public async Task<IActionResult> Delete(int id, string? tab = "matches")
        {
            var match = await _context.MatchInfo.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            var viewModel = new MatchViewModel
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

            ViewBag.ReturnTab = tab;
            return View(viewModel);
        }

        // POST: TournamentsList/DeleteConfirmed2/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed2(int id, string? tab = "matches")
        {
            var match = await _context.MatchInfo.FindAsync(id);
            if (match != null)
            {
                match.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Match deleted successfully!";
            }

            return RedirectToAction(nameof(Index), new { tab = tab });
        }

        // === TOURNAMENT DETAILS (from Tournaments tab) ===
        public async Task<IActionResult> TournamentDetails(int id, string? tab = "tournaments")
        {
            var tournament = await _context.Tournaments
                .Include(t => t.InvitedFacilities)
                .FirstOrDefaultAsync(t => t.TournamentID == id);

            if (tournament == null) return NotFound();

            ViewBag.ReturnTab = tab;
            return View(tournament);
        }

        // === TOURNAMENT EDIT ===
        [HttpGet]
        public async Task<IActionResult> TournamentEdit(int? id, string? tab = "tournaments")
        {
            if (id == null) return NotFound();

            var tournament = await _context.Tournaments
                .Include(t => t.InvitedFacilities)
                .FirstOrDefaultAsync(m => m.TournamentID == id);

            if (tournament == null) return NotFound();

            var viewModel = new TournamentViewModel
            {
                TournamentID = tournament.TournamentID,
                TournamentName = tournament.TournamentName,
                TournamentDescription = tournament.TournamentDescription,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Status = tournament.Status,
                OpenToEveryone = tournament.OpenToEveryone,
                Branch = tournament.Branch,
                IsActive = tournament.IsActive,
                InvitedFacilities = tournament.InvitedFacilities?.Select(f => new InvitedFacilityViewModel
                {
                    Invitedfacility = f.Invitedfacility,
                    InvitationMessage = f.InvitationMessage
                }).ToList() ?? new List<InvitedFacilityViewModel>()
            };

            ViewBag.ReturnTab = tab;
            return View("TournamentEdit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TournamentEdit(int id, TournamentViewModel model, string? tab = "tournaments")
        {
            if (id != model.TournamentID) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var tournament = await _context.Tournaments
                        .Include(t => t.InvitedFacilities)
                        .FirstOrDefaultAsync(m => m.TournamentID == id);
                    if (tournament == null) return NotFound();

                    tournament.TournamentName = model.TournamentName;
                    tournament.TournamentDescription = model.TournamentDescription;
                    tournament.StartDate = model.StartDate;
                    tournament.EndDate = model.EndDate;
                    tournament.Status = model.Status;
                    tournament.OpenToEveryone = model.OpenToEveryone;
                    tournament.Branch = model.Branch;
                    tournament.IsActive = model.IsActive;

                    _context.InvitedFacilities.RemoveRange(tournament.InvitedFacilities);
                    if (model.InvitedFacilities != null)
                    {
                        foreach (var facility in model.InvitedFacilities)
                        {
                            if (!string.IsNullOrEmpty(facility.Invitedfacility))
                            {
                                _context.InvitedFacilities.Add(new InvitedFacility
                                {
                                    TournamentID = tournament.TournamentID,
                                    Invitedfacility = facility.Invitedfacility,
                                    InvitationMessage = facility.InvitationMessage
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tournament updated successfully!";
                    return RedirectToAction(nameof(Index), new { tab });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating tournament");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.ReturnTab = tab;
            return View("TournamentEdit", model);
        }

        // GET: Tournament/TournamentDelete/5
        public async Task<IActionResult> TournamentDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .Include(t => t.InvitedFacilities)
                .FirstOrDefaultAsync(m => m.TournamentID == id);

            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        // === TOURNAMENT DELETE ===
        [HttpPost, ActionName("TournamentDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TournamentDeleteConfirmed(int id, string? tab = "tournaments")
        {
            var tournament = await _context.Tournaments
                .Include(t => t.InvitedFacilities)
                .FirstOrDefaultAsync(m => m.TournamentID == id);

            if (tournament != null)
            {
                if (tournament.InvitedFacilities?.Any() == true)
                    _context.InvitedFacilities.RemoveRange(tournament.InvitedFacilities);
                _context.Tournaments.Remove(tournament);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tournament deleted successfully!";
            }

            return RedirectToAction(nameof(Index), new { tab });
        }

        [HttpGet]
        public IActionResult TeamCreate(string? tab = "teams")
        {
            ViewBag.ReturnTab = tab;
            return View(new TeamViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeamCreate(TeamViewModel model, string? tab = "teams")
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var team = new Team
                    {
                        TeamName = model.TeamName,
                        TrainingActivity = model.TrainingActivity,
                        Branch = model.Branch,
                        Stadium = model.Stadium,
                        Coach = model.Coach,
                        Status = model.Status,
                        FoundationDate = model.FoundationDate,
                        TeamDescription = model.TeamDescription,
                        IsActive = model.IsActive,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };

                    if (model.TeamLogo != null && model.TeamLogo.Length > 0)
                    {
                        if (model.TeamLogo.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("TeamLogo", "File size cannot exceed 5MB.");
                            ViewBag.ReturnTab = tab;
                            return View(model);
                        }
                        if (!model.TeamLogo.ContentType.StartsWith("image/"))
                        {
                            ModelState.AddModelError("TeamLogo", "Please upload an image file.");
                            ViewBag.ReturnTab = tab;
                            return View(model);
                        }

                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "teamlogos");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.TeamLogo.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.TeamLogo.CopyToAsync(fileStream);
                        }

                        team.TeamLogoPath = "/Uploads/teamlogos/" + uniqueFileName;
                    }

                    _context.Teams.Add(team);
                    await _context.SaveChangesAsync();

                    //TempData["SuccessMessage"] = "Team created successfully!";

                    // Use absolute redirect to ensure clean navigation
                    return RedirectToAction("Index", new { tab = "teams" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating team");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.ReturnTab = tab;
            return View(model);
        }
        // Team Edit
        [HttpGet]
        public async Task<IActionResult> TeamEdit(int id, string? tab = "teams")
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();

            var viewModel = new TeamViewModel
            {
                TeamID = team.TeamID,
                TeamName = team.TeamName,
                TrainingActivity = team.TrainingActivity,
                Branch = team.Branch,
                TeamLogoPath = team.TeamLogoPath,
                Stadium = team.Stadium,
                Coach = team.Coach,
                Status = team.Status,
                FoundationDate = team.FoundationDate,
                TeamDescription = team.TeamDescription,
                IsActive = team.IsActive
            };

            ViewBag.ReturnTab = tab;
            return View("TeamEdit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeamEdit(int id, TeamViewModel model, string? tab = "teams")
        {
            if (id != model.TeamID) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var team = await _context.Teams.FindAsync(id);
                    if (team == null) return NotFound();

                    team.TeamName = model.TeamName;
                    team.TrainingActivity = model.TrainingActivity;
                    team.Branch = model.Branch;
                    team.Stadium = model.Stadium;
                    team.Coach = model.Coach;
                    team.Status = model.Status;
                    team.FoundationDate = model.FoundationDate;
                    team.TeamDescription = model.TeamDescription;
                    team.IsActive = model.IsActive;
                    team.ModifiedDate = DateTime.UtcNow; // Set on update

                    if (model.TeamLogo != null && model.TeamLogo.Length > 0)
                    {
                        if (model.TeamLogo.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("TeamLogo", "File size cannot exceed 5MB.");
                            ViewBag.ReturnTab = tab;
                            return View("TeamEdit", model);
                        }
                        if (!model.TeamLogo.ContentType.StartsWith("image/"))
                        {
                            ModelState.AddModelError("TeamLogo", "Please upload an image file.");
                            ViewBag.ReturnTab = tab;
                            return View("TeamEdit", model);
                        }

                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "teamlogos");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.TeamLogo.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.TeamLogo.CopyToAsync(fileStream);
                        }

                        // Delete old logo if it exists
                        if (!string.IsNullOrEmpty(team.TeamLogoPath))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", team.TeamLogoPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        team.TeamLogoPath = "/Uploads/teamlogos/" + uniqueFileName;
                        model.TeamLogoPath = team.TeamLogoPath;
                    }

                    _context.Update(team);
                    await _context.SaveChangesAsync();

                    //TempData["SuccessMessage"] = "Team updated successfully!";
                    return RedirectToAction("Index", new { tab });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating team");
                    TempData["ErrorMessage"] = $"Error: {ex.Message}";
                }
            }

            ViewBag.ReturnTab = tab;
            return View("TeamEdit", model);
        }

        [HttpGet]
        public async Task<IActionResult> TeamDetails(int id, string? tab = "teams")
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                TempData["ErrorMessage"] = "Team not found.";
                return RedirectToAction("Index", new { tab });
            }

            var viewModel = new TeamViewModel
            {
                TeamID = team.TeamID,
                TeamName = team.TeamName,
                TrainingActivity = team.TrainingActivity,
                Branch = team.Branch,
                TeamLogoPath = team.TeamLogoPath,
                Stadium = team.Stadium,
                Coach = team.Coach,
                Status = team.Status,
                FoundationDate = team.FoundationDate,
                TeamDescription = team.TeamDescription,
                IsActive = team.IsActive
            };

            ViewBag.ReturnTab = tab;
            return View("TeamDetails", viewModel);
        }

        // Team Delete - GET
        [HttpGet]
        public async Task<IActionResult> TeamDelete(int id, string? tab = "teams")
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                TempData["ErrorMessage"] = "Team not found.";
                return RedirectToAction("Index", new { tab });
            }

            var viewModel = new TeamViewModel
            {
                TeamID = team.TeamID,
                TeamName = team.TeamName,
                TrainingActivity = team.TrainingActivity,
                Branch = team.Branch,
                TeamLogoPath = team.TeamLogoPath,
                Stadium = team.Stadium,
                Coach = team.Coach,
                Status = team.Status,
                FoundationDate = team.FoundationDate,
                TeamDescription = team.TeamDescription,
                IsActive = team.IsActive
            };

            ViewBag.ReturnTab = tab;
            return View("TeamDelete", viewModel);
        }

        // Team Delete - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeamDeleteConfirmed(int id, string? tab = "teams")
        {
            try
            {
                var team = await _context.Teams.FindAsync(id);
                if (team == null)
                {
                    TempData["ErrorMessage"] = "Team not found.";
                    return RedirectToAction("Index", new { tab });
                }

                // Delete the team logo file if it exists
                if (!string.IsNullOrEmpty(team.TeamLogoPath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", team.TeamLogoPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();

                //TempData["SuccessMessage"] = "Team deleted successfully!";
                return RedirectToAction("Index", new { tab });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team");
                TempData["ErrorMessage"] = $"Error deleting team: {ex.Message}";
                return RedirectToAction("Index", new { tab });
            }
        }

        // GET: TournamentsList/Delete/5
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var match = await _context.MatchInfo.FindAsync(id);
        //    if (match == null)
        //    {
        //        return NotFound();
        //    }

        //    var viewModel = new MatchViewModel
        //    {
        //        MatchId = match.MatchId,
        //        Tournament = match.Tournament,
        //        HomeTeam = match.HomeTeam,
        //        AwayTeam = match.AwayTeam,
        //        MatchDate = match.MatchDate,
        //        MatchTime = match.MatchTime,
        //        Venue = match.Venue,
        //        Referee = match.Referee,
        //        HomeScore = match.HomeScore,
        //        AwayScore = match.AwayScore,
        //        Winner = match.Winner,
        //        MatchStatus = match.MatchStatus,
        //        VIPGuests = match.VIPGuests,
        //        Sponsors = match.Sponsors,
        //        InvitationNotes = match.InvitationNotes
        //    };

        //    return View(viewModel);
        //}

        //// POST: TournamentsList/DeleteConfirmed2/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed2(int id)
        //{
        //    var match = await _context.MatchInfo.FindAsync(id);
        //    if (match != null)
        //    {
        //        match.IsActive = false;
        //        await _context.SaveChangesAsync();
        //        TempData["SuccessMessage"] = "Match deleted successfully!";
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        // === HELPER ===
        private async Task<bool> MatchExists(int id) =>
            await _context.MatchInfo.AnyAsync(e => e.MatchId == id);

        // Optional: Redirect to Index with tournaments tab
        public IActionResult ListTournament() =>
            RedirectToAction(nameof(Index), new { tab = "tournaments" });

        // Optional: Redirect to Index with matches tab 
        public IActionResult ListMatches() =>
            RedirectToAction(nameof(Index), new { tab = "matches" });
    }
}