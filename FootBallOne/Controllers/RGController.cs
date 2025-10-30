using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;
using FootBallOne.Models;

namespace FootBallOne.Controllers
{
    public class RGController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RGController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RG
        public async Task<IActionResult> Index()
        {
            return View(await _context.RGManagements.ToListAsync());
        }

        // GET: RG/Details/5
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

        // GET: RG/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RG/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,TotalFees,Email,Address,City,ImageURL,CoachName,WorkingHours,PaymentStatus")] RegistrationManagement registrationManagement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(registrationManagement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(registrationManagement);
        }

        // GET: RG/Edit/5
        public async Task<IActionResult> Edit(int? id)
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

        // POST: RG/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PhoneNo,Category,SubscriptionStart,SubscriptionEnd,SubscriptionFees,KitFees,SugarFees,BagFees,TotalFees,Email,Address,City,ImageURL,CoachName,WorkingHours,PaymentStatus")] RegistrationManagement registrationManagement)
        {
            if (id != registrationManagement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registrationManagement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistrationManagementExists(registrationManagement.Id))
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
            return View(registrationManagement);
        }

        // GET: RG/Delete/5
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

        // POST: RG/Delete/5
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

        private bool RegistrationManagementExists(int id)
        {
            return _context.RGManagements.Any(e => e.Id == id);
        }
    }
}
