using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

public class CoachDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoachDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ✅ Show Present Players - Filtered
    //public IActionResult Today(string coachName, string category, string timeSlot)
    //{
    //    var today = DateTime.Today;

    //    var players = _context.RGManagements
    //        .Where(p => p.SubscriptionEnd >= today && p.PaymentStatus == "Paid");

    //    if (!string.IsNullOrEmpty(coachName))
    //        players = players.Where(p => p.CoachName == coachName);
    //    if (!string.IsNullOrEmpty(category))
    //        players = players.Where(p => p.Category == category);
    //    if (!string.IsNullOrEmpty(timeSlot))
    //        players = players.Where(p => p.TimeSlot == timeSlot);

    //    return View(players.ToList());
    //}

    public IActionResult Today(string coachName, string category, string timeSlot)
    {
        var today = DateTime.Today;

        // Load players who are paid and active
        var players = _context.RGManagements
            .Where(p => p.SubscriptionEnd >= today && p.PaymentStatus == "Paid");

        // Filter by Coach Name
        if (!string.IsNullOrEmpty(coachName))
            players = players.Where(p => p.CoachName == coachName);

        // Filter by Category
        if (!string.IsNullOrEmpty(category))
            players = players.Where(p => p.Category == category);

        // Filter by TimeSlot
        if (!string.IsNullOrEmpty(timeSlot))
            players = players.Where(p => p.TimeSlot == timeSlot);

        // Populate dropdown list for Coaches (for view)
        ViewBag.Coaches = _context.Coaches
            .Select(c => new SelectListItem
            {
                Text = c.FullName,
                Value = c.FullName
            }).ToList();

        return View(players.ToList());
    }

    // ✅ List of Players Who Didn't Renew
    public IActionResult Stopped()
    {
        var players = _context.RGManagements
            .Where(p => p.SubscriptionEnd < DateTime.Today)
            .ToList();
        return View(players);
    }

    // ✅ List of Players Whose Renewal Is Due Soon
    public IActionResult Renew()
    {
        var today = DateTime.Today;
        var players = _context.RGManagements
            .Where(p => p.SubscriptionEnd >= today && p.SubscriptionEnd <= today.AddDays(5))
            .ToList();
        return View(players);
    }

    // ✅ Attendance GET
    public IActionResult MarkAttendance(string coachName, string category, string timeSlot)
    {
        var today = DateTime.Today;

        var players = _context.RGManagements
            .Where(p => p.SubscriptionEnd >= today && p.PaymentStatus == "Paid");

        if (!string.IsNullOrEmpty(coachName))
            players = players.Where(p => p.CoachName == coachName);
        if (!string.IsNullOrEmpty(category))
            players = players.Where(p => p.Category == category);
        if (!string.IsNullOrEmpty(timeSlot))
            players = players.Where(p => p.TimeSlot == timeSlot);

        var attendanceList = players
            .Select(p => new CoachAttendance
            {
                PlayerName = p.Name,
                CoachName = p.CoachName,
                Date = today
            }).ToList();

        return View(attendanceList);
    }

    // ✅ Attendance POST
    [HttpPost]
    public IActionResult MarkAttendance(List<CoachAttendance> attendances)
    {
        if (attendances != null && attendances.Any())
        {
            _context.CoachAttendances.AddRange(attendances);

            try
            {
                _context.SaveChanges();
                TempData["Success"] = "✅ Attendance saved successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "⚠️ Failed to save attendance: " + ex.Message;
            }
        }
        else
        {
            TempData["Error"] = "⚠️ No attendance data to save.";
        }

        return RedirectToAction("MarkAttendance");
    }
}
