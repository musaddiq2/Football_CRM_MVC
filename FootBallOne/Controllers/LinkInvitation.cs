using FootBallOne.Data;
using FootBallOne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace FootBallOne.Controllers
{
    public class LinkInvitation : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LinkInvitation> _logger;

        public LinkInvitation(ApplicationDbContext context, ILogger<LinkInvitation> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper method to get AcademyID from session
        private async Task<int?> GetAcademyIdAsync()
        {
            int? academyId = HttpContext.Session.GetInt32("AcademyID");

            if (academyId == null || academyId == 0)
            {
                var adminEmail = HttpContext.Session.GetString("AdminEmail");
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    var admin = await _context.Admintbl.FirstOrDefaultAsync(a => a.Email == adminEmail);
                    if (admin != null && admin.AcademyID != null)
                        academyId = admin.AcademyID;
                }
            }

            return academyId;
        }

        // GET: /LinkInvitation/Index (Generate Link Form)
        public IActionResult Index()
        {
            var model = new GenerateLinkModel();
            return View(model);
        }

        // POST: /LinkInvitation/Index (Handle Form Submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(GenerateLinkModel model)
        {
            if (ModelState.IsValid)
            {
                // Get AcademyID
                var academyId = await GetAcademyIdAsync();

                var link = new InvitationLink
                {
                    Name = model.Name,
                    Description = model.Description,
                    Token = Guid.NewGuid().ToString("N").Substring(0, 32),
                    ExpiresAt = GetExpirationDate(model.ExpirationPeriod),
                    ShareTitle = model.ShareTitle,
                    ShareDescription = model.ShareDescription,
                    ShareImage = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AcademyID = academyId // Set academy ID
                };

                if (model.Image != null)
                {
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                    var filePath = Path.Combine(imagesPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(stream);
                    }
                    link.ShareImage = $"/images/{fileName}";
                }

                _context.InvitationLinks.Add(link);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Invitation link created: {link.Name} (ID: {link.Id}, AcademyID: {academyId})");

                return RedirectToAction("List");
            }
            return View(model);
        }

        private DateTime? GetExpirationDate(string period)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            return period switch
            {
                "1 Day" => now.AddDays(1),
                "3 Days" => now.AddDays(3),
                "1 Week" => now.AddDays(7),
                "1 Month" => now.AddMonths(1),
                _ => null
            };
        }

        // GET: /LinkInvitation/List (Display Invitation Links) - FILTERED BY ACADEMY
        public async Task<IActionResult> List()
        {
            try
            {
                var academyId = await GetAcademyIdAsync();

                var linksQuery = _context.InvitationLinks.AsQueryable();

                // Filter by AcademyID if present
                if (academyId.HasValue && academyId.Value > 0)
                {
                    linksQuery = linksQuery.Where(l => l.AcademyID == academyId.Value);
                }

                var links = await linksQuery
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();

                if (links == null || !links.Any())
                {
                    _logger.LogWarning($"No invitation links found for AcademyID: {academyId}");
                }

                return View(links);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving invitation links.");
                return View(new List<InvitationLink>());
            }
        }

        // GET: /LinkInvitation/Edit/{id} (Edit Link Form)
        public async Task<IActionResult> Edit(int id)
        {
            var academyId = await GetAcademyIdAsync();

            var linkQuery = _context.InvitationLinks.AsQueryable();

            // Filter by AcademyID for security
            if (academyId.HasValue && academyId.Value > 0)
            {
                linkQuery = linkQuery.Where(l => l.AcademyID == academyId.Value);
            }

            var link = await linkQuery.FirstOrDefaultAsync(l => l.Id == id);

            if (link == null)
            {
                return NotFound();
            }

            string expirationPeriod = "1 Week";
            if (link.ExpiresAt.HasValue)
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                var timeSpan = link.ExpiresAt.Value - now;

                if (timeSpan.Days <= 1)
                    expirationPeriod = "1 Day";
                else if (timeSpan.Days <= 3)
                    expirationPeriod = "3 Days";
                else if (timeSpan.Days <= 7)
                    expirationPeriod = "1 Week";
                else if (timeSpan.Days <= 31)
                    expirationPeriod = "1 Month";
                else
                    expirationPeriod = "Custom";
            }

            var model = new EditLinkModel
            {
                Name = link.Name,
                Description = link.Description,
                ShareTitle = link.ShareTitle,
                ShareDescription = link.ShareDescription,
                ExpirationPeriod = expirationPeriod
            };

            ViewBag.LinkId = id;
            ViewBag.CurrentImage = link.ShareImage;
            ViewBag.CurrentExpiration = expirationPeriod;

            return View("Edit", model);
        }

        // POST: /LinkInvitation/Edit/{id} (Update Link)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditLinkModel model)
        {
            if (ModelState.ContainsKey("Image"))
            {
                ModelState["Image"].Errors.Clear();
                ModelState["Image"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            _logger.LogInformation($"Edit POST called for ID: {id}");

            if (ModelState.IsValid)
            {
                var academyId = await GetAcademyIdAsync();
                var linkQuery = _context.InvitationLinks.AsQueryable();

                // Filter by AcademyID for security
                if (academyId.HasValue && academyId.Value > 0)
                {
                    linkQuery = linkQuery.Where(l => l.AcademyID == academyId.Value);
                }

                var link = await linkQuery.FirstOrDefaultAsync(l => l.Id == id);

                if (link == null)
                {
                    _logger.LogWarning($"Link not found or unauthorized for ID: {id}");
                    return NotFound();
                }

                link.Name = model.Name;
                link.Description = model.Description;
                link.ShareTitle = model.ShareTitle;
                link.ShareDescription = model.ShareDescription;
                link.ExpiresAt = GetExpirationDate(model.ExpirationPeriod);
                link.UpdatedAt = DateTime.UtcNow;

                if (model.Image != null)
                {
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    if (!string.IsNullOrEmpty(link.ShareImage))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", link.ShareImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Could not delete old image file");
                            }
                        }
                    }

                    var fileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                    var filePath = Path.Combine(imagesPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(stream);
                    }
                    link.ShareImage = $"/images/{fileName}";
                }

                try
                {
                    _context.InvitationLinks.Update(link);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Invitation link updated: {link.Name} (ID: {link.Id})");

                    TempData["SuccessMessage"] = "Link updated successfully!";
                    return RedirectToAction("List");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error saving link update for ID: {id}");
                    ModelState.AddModelError("", "An error occurred while saving the changes.");
                }
            }

            ViewBag.LinkId = id;
            var existingLink = await _context.InvitationLinks.FindAsync(id);
            ViewBag.CurrentImage = existingLink?.ShareImage;
            ViewBag.CurrentExpiration = model.ExpirationPeriod;

            return View("Edit", model);
        }

        // POST: /LinkInvitation/Deactivate/{id}
        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var academyId = await GetAcademyIdAsync();
                var linkQuery = _context.InvitationLinks.AsQueryable();

                if (academyId.HasValue && academyId.Value > 0)
                {
                    linkQuery = linkQuery.Where(l => l.AcademyID == academyId.Value);
                }

                var link = await linkQuery.FirstOrDefaultAsync(l => l.Id == id);

                if (link == null)
                {
                    return Json(new { success = false, message = "Link not found" });
                }

                link.IsActive = false;
                link.UpdatedAt = DateTime.UtcNow;
                _context.InvitationLinks.Update(link);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Invitation link deactivated: {link.Name} (ID: {link.Id})");

                return Json(new { success = true, message = "Link deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating link");
                return Json(new { success = false, message = "Error deactivating link" });
            }
        }

        // POST: /LinkInvitation/Activate/{id}
        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var academyId = await GetAcademyIdAsync();
                var linkQuery = _context.InvitationLinks.AsQueryable();

                if (academyId.HasValue && academyId.Value > 0)
                {
                    linkQuery = linkQuery.Where(l => l.AcademyID == academyId.Value);
                }

                var link = await linkQuery.FirstOrDefaultAsync(l => l.Id == id);

                if (link == null)
                {
                    return Json(new { success = false, message = "Link not found" });
                }

                link.IsActive = true;
                link.UpdatedAt = DateTime.UtcNow;
                _context.InvitationLinks.Update(link);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Invitation link activated: {link.Name} (ID: {link.Id})");

                return Json(new { success = true, message = "Link activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating link");
                return Json(new { success = false, message = "Error activating link" });
            }
        }

        // POST: /LinkInvitation/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var academyId = await GetAcademyIdAsync();
                var linkQuery = _context.InvitationLinks.AsQueryable();

                if (academyId.HasValue && academyId.Value > 0)
                {
                    linkQuery = linkQuery.Where(l => l.AcademyID == academyId.Value);
                }

                var link = await linkQuery.FirstOrDefaultAsync(l => l.Id == id);

                if (link == null)
                {
                    return Json(new { success = false, message = "Link not found" });
                }

                _context.InvitationLinks.Remove(link);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Invitation link deleted: {link.Name} (ID: {link.Id})");

                return Json(new { success = true, message = "Link deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting link");
                return Json(new { success = false, message = "Error deleting link" });
            }
        }

        // GET: /LinkInvitation/ShowQR/{id}
        public async Task<IActionResult> ShowQR(int id)
        {
            var link = await _context.InvitationLinks.FindAsync(id);
            if (link == null)
            {
                return NotFound();
            }

            var fullUrl = Url.Action(
                "RequestRegistration",
                "RegistrationManagements",
                new { token = link.Token },
                Request.Scheme
            );

            ViewBag.QRCodeUrl = $"/LinkInvitation/GenerateQRCode/{id}";
            ViewBag.FullRegistrationUrl = fullUrl;

            _logger.LogInformation($"QR code viewed for link: {link.Name} (ID: {link.Id})");

            return View(link);
        }

        // GET: /LinkInvitation/GenerateQRCode/{id}
        public async Task<IActionResult> GenerateQRCode(int id)
        {
            var link = await _context.InvitationLinks.FindAsync(id);
            if (link == null)
            {
                return NotFound();
            }

            var fullUrl = Url.Action(
                "RequestRegistration",
                "RegistrationManagements",
                new { token = link.Token },
                Request.Scheme
            );

            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(fullUrl, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        using (var bitmap = qrCode.GetGraphic(20))
                        {
                            using (var stream = new MemoryStream())
                            {
                                bitmap.Save(stream, ImageFormat.Png);
                                return File(stream.ToArray(), "image/png");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return BadRequest("Error generating QR code");
            }
        }

        // GET: /LinkInvitation/DownloadQRImage/{id}
        public async Task<IActionResult> DownloadQRImage(int id)
        {
            var link = await _context.InvitationLinks.FindAsync(id);
            if (link == null)
            {
                return NotFound();
            }

            var fullUrl = Url.Action(
                "RequestRegistration",
                "RegistrationManagements",
                new { token = link.Token },
                Request.Scheme
            );

            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(fullUrl, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        using (var bitmap = qrCode.GetGraphic(20))
                        {
                            using (var stream = new MemoryStream())
                            {
                                bitmap.Save(stream, ImageFormat.Png);
                                var fileName = $"QR_{link.Name.Replace(" ", "")}{DateTime.Now:yyyyMMddHHmmss}.png";

                                _logger.LogInformation($"QR image downloaded for link: {link.Name} (ID: {link.Id})");

                                return File(stream.ToArray(), "image/png", fileName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading QR image");
                return BadRequest("Error downloading QR image");
            }
        }

        // GET: /LinkInvitation/DownloadQRBadge/{id}
        public async Task<IActionResult> DownloadQRBadge(int id)
        {
            var link = await _context.InvitationLinks.FindAsync(id);
            if (link == null)
            {
                return NotFound();
            }

            var fullUrl = Url.Action(
                "RequestRegistration",
                "RegistrationManagements",
                new { token = link.Token },
                Request.Scheme
            );

            try
            {
                int width = 600;
                int height = 800;

                using (var bitmap = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.White);
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    using (var headerBrush = new SolidBrush(Color.FromArgb(0, 123, 255)))
                    {
                        graphics.FillRectangle(headerBrush, 0, 0, width, 150);
                    }

                    using (var titleFont = new Font("Arial", 24, FontStyle.Bold))
                    using (var titleBrush = new SolidBrush(Color.White))
                    {
                        var titleFormat = new StringFormat { Alignment = StringAlignment.Center };
                        graphics.DrawString("Invitation Link", titleFont, titleBrush,
                            new RectangleF(0, 50, width, 50), titleFormat);
                    }

                    using (var qrGenerator = new QRCodeGenerator())
                    {
                        var qrCodeData = qrGenerator.CreateQrCode(fullUrl, QRCodeGenerator.ECCLevel.Q);
                        using (var qrCode = new QRCode(qrCodeData))
                        using (var qrBitmap = qrCode.GetGraphic(20))
                        {
                            int qrSize = 400;
                            int qrX = (width - qrSize) / 2;
                            int qrY = 200;
                            graphics.DrawImage(qrBitmap, qrX, qrY, qrSize, qrSize);
                        }
                    }

                    using (var nameFont = new Font("Arial", 18, FontStyle.Bold))
                    using (var nameBrush = new SolidBrush(Color.Black))
                    {
                        var nameFormat = new StringFormat { Alignment = StringAlignment.Center };
                        graphics.DrawString(link.Name, nameFont, nameBrush,
                            new RectangleF(0, 620, width, 40), nameFormat);
                    }

                    if (!string.IsNullOrEmpty(link.Description))
                    {
                        using (var descFont = new Font("Arial", 12))
                        using (var descBrush = new SolidBrush(Color.Gray))
                        {
                            var descFormat = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center
                            };
                            graphics.DrawString(link.Description, descFont, descBrush,
                                new RectangleF(50, 670, width - 100, 80), descFormat);
                        }
                    }

                    using (var stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        var fileName = $"Badge_{link.Name.Replace(" ", "")}{DateTime.Now:yyyyMMddHHmmss}.png";

                        _logger.LogInformation($"QR badge downloaded for link: {link.Name} (ID: {link.Id})");

                        return File(stream.ToArray(), "image/png", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR badge");
                return BadRequest("Error generating QR badge");
            }
        }

        public IActionResult Success()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public string Message { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}