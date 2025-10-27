// CONTROLLER: PlayerRegistrationController.cs
using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FootBallOne.Controllers
{
    public class PlayerRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlayerRegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // STEP 1: Online Registration Form (used by players)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Login login)
        {
            if (ModelState.IsValid)
            {
                login.Password = "default"; // optional: encrypt or handle password
                _context.Logintbl.Add(login);
                _context.SaveChanges();
                ViewBag.Message = "Registration request submitted successfully!";
                return RedirectToAction("Register");
            }
            return View(login);
        }

        // STEP 2: Admin sees New Requests
        public IActionResult NewRequests()
        {
            var requests = _context.Logintbl.ToList();
            return View(requests);
        }

        public IActionResult Accept(int id)
        {
            var player = _context.Logintbl.Find(id);
            if (player != null)
            {
                var registration = new RegistrationManagement
                {
                    Name = player.Name,
                    PhoneNo = player.PhoneNo,
                    Email = player.Email,
                    City = "",
                    CreatedDate = DateTime.Now,
                    AcademyID = player.AcademyID
                };
                _context.RGManagements.Add(registration);
                _context.Logintbl.Remove(player);
                _context.SaveChanges();
                return RedirectToAction("NewRequests");
            }
            return NotFound();
        }

        public IActionResult Reject(int id)
        {
            var player = _context.Logintbl.Find(id);
            if (player != null)
            {
                _context.Logintbl.Remove(player);
                _context.SaveChanges();
            }
            return RedirectToAction("NewRequests");
        }

        // STEP 3: Admin completes registration
        public IActionResult CompleteRegister()
        {
            var list = _context.RGManagements.ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var data = _context.RGManagements.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(RegistrationManagement player)
        {
            if (ModelState.IsValid)
            {
                _context.RGManagements.Update(player);
                _context.SaveChanges();
                return RedirectToAction("CompleteRegister");
            }
            return View(player);
        }

        // STEP 4: Renew Subscription
        [HttpGet]
        public IActionResult Renew(int id)
        {
            var player = _context.RGManagements.Find(id);
            return View(player);
        }

        [HttpPost]
        public IActionResult Renew(RegistrationManagement updatedPlayer)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.RGManagements.Find(updatedPlayer.Id);
                if (existing != null)
                {
                    existing.SubscriptionStart = updatedPlayer.SubscriptionStart;
                    existing.SubscriptionEnd = updatedPlayer.SubscriptionEnd;
                    existing.PaymentStatus = updatedPlayer.PaymentStatus;
                    _context.SaveChanges();
                }
                return RedirectToAction("CompleteRegister");
            }
            return View(updatedPlayer);
        }
    }
}
