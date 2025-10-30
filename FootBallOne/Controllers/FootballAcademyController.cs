using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FootBallOne.Data;

namespace FootBallOne.Controllers
{
    public class FootballAcademyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FootballAcademyController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Academy
        public async Task<IActionResult> Index()
        {
            var academies = await _context.FootballAcademy.ToListAsync();
            return View(academies);
        }

        // GET: Academy/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Academy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Academy academy)
        {
            if (ModelState.IsValid)
            {
                _context.FootballAcademy.Add(academy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(academy);
        }

        // GET: Academy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var academy = await _context.FootballAcademy.FindAsync(id);
            if (academy == null)
                return NotFound();

            return View(academy);
        }

        // POST: Academy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Academy academy)
        {
            if (id != academy.AcademyID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(academy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FootballAcademy.Any(e => e.AcademyID == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(academy);
        }

        // GET: Academy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var academy = await _context.FootballAcademy
                .FirstOrDefaultAsync(m => m.AcademyID == id);

            if (academy == null)
                return NotFound();

            return View(academy);
        }

        // POST: Academy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var academy = await _context.FootballAcademy.FindAsync(id);
            if (academy != null)
            {
                _context.FootballAcademy.Remove(academy);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
