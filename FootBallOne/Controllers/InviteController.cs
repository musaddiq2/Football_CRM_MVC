using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FootBallOne.Controllers
{
    public class InviteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InviteController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: /invite/{token}
        public async Task<IActionResult> Index(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound("No invitation token provided.");
            }

            var utcNow = DateTime.UtcNow;
            var invitation = await _context.InvitationLinks
                .Where(i => i.Token == token)
                .Where(i => i.IsActive)
                .Where(i => !i.ExpiresAt.HasValue || i.ExpiresAt > utcNow)
                .Where(i => !i.MaxUsage.HasValue || i.UsageCount < i.MaxUsage.Value)
                .FirstOrDefaultAsync();

            if (invitation == null)
            {
                return NotFound("Invalid or expired invitation link.");
            }

            // Increment usage
            invitation.IncrementUsage();
            await _context.SaveChangesAsync();

            // Redirect to RequestRegistration with the token
            return RedirectToAction("RequestRegistration", "RegistrationManagements", new { token = token });
        }
    }
}