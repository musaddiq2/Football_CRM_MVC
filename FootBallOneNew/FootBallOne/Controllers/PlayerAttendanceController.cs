using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FootBallOne.Controllers
{
    public class PlayerAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlayerAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Mark Attendance - Displays players for a selected coach
        public IActionResult MarkAttendance(string? coachName)
        {
            // Retrieve distinct coach names from the players table (RGManagements)
            var coaches = _context.RGManagements
                .Where(r => !string.IsNullOrEmpty(r.CoachName))
                .Select(r => r.CoachName)
                .Distinct()
                .ToList();

            var players = new List<RegistrationManagement>();

            if (!string.IsNullOrEmpty(coachName))
            {
                players = _context.RGManagements
                    .Where(r => r.CoachName == coachName)
                    .ToList();
            }

            ViewBag.Coaches = new SelectList(coaches, coachName);
            ViewBag.SelectedCoach = coachName;

            return View(players);
        }

        // POST: Mark Attendance - Save attendance for players
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAttendance(List<int> presentPlayerIds, string coachName)
        {
            var today = DateTime.Today;

            // Retrieve all players belonging to the provided coach.
            var players = _context.RGManagements
                .Where(p => p.CoachName == coachName)
                .ToList();

            foreach (var player in players)
            {
                // Prevent duplicate marking: if an attendance record for today exists, skip it.
                bool alreadyMarked = _context.PlayerAttendances.Any(a =>
                    a.PlayerId == player.Id && a.Date.Date == today);

                if (alreadyMarked)
                    continue;

                // Decide status based on presence in the submitted list.
                var status = presentPlayerIds.Contains(player.Id) ? "Present" : "Absent";

                var attendance = new PlayerAttendance
                {
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    CoachName = coachName,
                    Status = status,
                    Date = today
                };

                _context.PlayerAttendances.Add(attendance);
            }

            _context.SaveChanges();
            TempData["Success"] = "Attendance saved successfully!";
            return RedirectToAction("MarkAttendance", new { coachName });
        }

        // GET: View Attendance - Filter attendance records by coach and date
        public IActionResult ViewAttendance(string coachName, DateTime? date)
        {
            var attendanceQuery = _context.PlayerAttendances.AsQueryable();

            if (!string.IsNullOrEmpty(coachName))
            {
                attendanceQuery = attendanceQuery.Where(a => a.CoachName == coachName);
            }

            if (date.HasValue)
            {
                attendanceQuery = attendanceQuery.Where(a => a.Date.Date == date.Value.Date);
                ViewBag.SelectedDate = date.Value.ToString("yyyy-MM-dd");
            }

            // Retrieve list of coaches for the filter dropdown.
            var coachList = _context.PlayerAttendances
                .Select(p => p.CoachName)
                .Distinct()
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();

            ViewBag.CoachList = coachList;
            ViewBag.SelectedCoach = coachName;

            return View(attendanceQuery.ToList());
        }

        // GET: Attendance Summary - Display summary grouped by date
        public IActionResult AttendanceSummary()
        {
            var summary = _context.PlayerAttendances
                .GroupBy(a => a.Date.Date)
                .Select(g => new AttendanceSummaryViewModel
                {
                    Date = g.Key,
                    TotalRecords = g.Count(),
                    TotalPresent = g.Count(a => a.Status == "Present"),
                    TotalAbsent = g.Count(a => a.Status == "Absent")
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return View(summary);
        }
    }
}
