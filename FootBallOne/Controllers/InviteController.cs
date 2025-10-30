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
        private readonly ILogger<InviteController> _logger;

        public InviteController(ApplicationDbContext context, ILogger<InviteController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /invite/{token}
        public async Task<IActionResult> Index(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Invitation accessed without token");
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
                _logger.LogWarning($"Invalid or expired invitation token: {token}");
                return View("InvalidInvitation");
            }

            // Log the invitation details including AcademyID
            _logger.LogInformation($"Valid invitation accessed - Token: {token}, Academy: {invitation.AcademyID}, Name: {invitation.Name}");

            // Increment usage counter
            invitation.IncrementUsage();
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Invitation usage incremented - Count: {invitation.UsageCount}/{invitation.MaxUsage?.ToString() ?? "unlimited"}");

            // Redirect to RequestRegistration with the token
            // The AcademyID will be captured in RequestRegistration method
            return RedirectToAction("RequestRegistration", "RegistrationManagements", new { token = token });
        }

        // Optional: Display invitation details before redirecting
        public async Task<IActionResult> Preview(string token)
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

            // Pass invitation details to view without incrementing usage
            ViewBag.InvitationName = invitation.Name;
            ViewBag.Description = invitation.Description;
            ViewBag.ShareImage = invitation.ShareImage;
            ViewBag.Token = token;

            return View(invitation);
        }
    }
}