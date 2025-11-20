// Controllers/HRController.cs
using Microsoft.AspNetCore.Mvc;
using CMCS_MVC_Prototype.Services;
using CMCS_MVC_Prototype.Models;
using CMCS_MVC_Prototype.Attributes;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CMCS_MVC_Prototype.Controllers
{
    [Authorize("HR")]
    public class HRController : Controller
    {
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IAuthService _authService;

        public HRController(IUserService userService, IClaimService claimService, IAuthService authService)
        {
            _userService = userService;
            _claimService = claimService;
            _authService = authService;
        }

        // GET: /HR
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // GET: /HR/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validate password
                    if (string.IsNullOrEmpty(password) || password.Length < 6)
                    {
                        TempData["ErrorMessage"] = "Password must be at least 6 characters long";
                        return View(user);
                    }

                    user.PasswordHash = _authService.HashPassword(password);

                    // Generate unique ID for ALL users (not just lecturers)
                    if (user.Role == "Lecturer")
                    {
                        user.GenerateLecturerId();
                    }
                    else
                    {
                        // Generate a staff ID for non-lecturers
                        user.GenerateStaffId();
                    }

                    await _userService.CreateUserAsync(user);
                    TempData["SuccessMessage"] = $"User created successfully! ID: {user.LecturerId}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating user: {ex.Message}";
                }
            }
            return View(user);
        }
        // GET: /HR/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // If user is not a lecturer, ensure hourly rate is 0
                    if (user.Role != "Lecturer")
                    {
                        user.HourlyRate = 0;
                    }

                    await _userService.UpdateUserAsync(user);
                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
                }
            }
            return View(user);
        }

        // POST: /HR/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _userService.DeactivateUserAsync(id);
                TempData["SuccessMessage"] = "User deactivated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deactivating user: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /HR/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _userService.ActivateUserAsync(id);
                TempData["SuccessMessage"] = "User activated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error activating user: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /HR/Reports
        public async Task<IActionResult> Reports()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            var users = await _userService.GetAllUsersAsync();

            var reportData = claims.GroupBy(c => c.Month)
                .Select(g => new ReportViewModel
                {
                    Month = g.Key,
                    TotalClaims = g.Count(),
                    TotalAmount = g.Sum(c => c.Total),
                    ApprovedClaims = g.Count(c => c.Status.Contains("Approved")),
                    PendingClaims = g.Count(c => c.Status == "Pending")
                }).ToList();

            // Add stats to ViewBag for the quick stats cards
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = claims.Count(c => c.Status == "Pending");
            ViewBag.TotalValue = claims.Sum(c => c.Total).ToString("F2");

            return View(reportData);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateInvoice(string month)
        {
            try
            {
                var claims = await _claimService.GetClaimsByMonthAsync(month);
                var pdfBytes = GeneratePdfReport(claims, month);

                return File(pdfBytes, "application/pdf", $"Invoice-Report-{month}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
                return RedirectToAction(nameof(Reports));
            }
        }

        private byte[] GeneratePdfReport(List<Claim> claims, string month)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Create PDF document
                var document = new iTextSharp.text.Document();
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add title and header
                var titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 18);
                var headerFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 12);
                var normalFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10);

                // Title
                var title = new iTextSharp.text.Paragraph("Contract Monthly Claim System", titleFont);
                title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                document.Add(title);

                // Subtitle
                var subtitle = new iTextSharp.text.Paragraph($"Invoice Report for {month}", headerFont);
                subtitle.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                document.Add(subtitle);

                // Generation date
                document.Add(new iTextSharp.text.Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont));
                document.Add(new iTextSharp.text.Paragraph(" ")); // Empty line for spacing

                // Summary section
                document.Add(new iTextSharp.text.Paragraph("Summary:", headerFont));
                document.Add(new iTextSharp.text.Paragraph($"Total Claims: {claims.Count}"));
                document.Add(new iTextSharp.text.Paragraph($"Total Amount: R {claims.Sum(c => c.Total):F2}"));
                document.Add(new iTextSharp.text.Paragraph($"Pending Claims: {claims.Count(c => c.Status == "Pending")}"));
                document.Add(new iTextSharp.text.Paragraph($"Approved Claims: {claims.Count(c => c.Status.Contains("Approved"))}"));
                document.Add(new iTextSharp.text.Paragraph(" ")); // Empty line for spacing

                // Create table
                var table = new iTextSharp.text.pdf.PdfPTable(7);
                table.WidthPercentage = 100;

                // Set column widths
                float[] columnWidths = new float[] { 2f, 1.5f, 1f, 1f, 1f, 1.5f, 1.5f };
                table.SetWidths(columnWidths);

                // Add table headers
                table.AddCell(new iTextSharp.text.Phrase("Lecturer Name", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Lecturer ID", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Hours", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Rate", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Total", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Status", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Submitted", headerFont));

                // Add data rows
                foreach (var claim in claims)
                {
                    table.AddCell(new iTextSharp.text.Phrase(claim.LecturerName, normalFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.LecturerId, normalFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.HoursWorked.ToString("F2"), normalFont));
                    table.AddCell(new iTextSharp.text.Phrase($"R {claim.HourlyRate:F2}", normalFont));
                    table.AddCell(new iTextSharp.text.Phrase($"R {claim.Total:F2}", normalFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.Status, normalFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.Submitted.ToString("yyyy-MM-dd"), normalFont));
                }

                document.Add(table);
                document.Add(new iTextSharp.text.Paragraph(" ")); // Empty line for spacing

                // Add grand total
                var totalParagraph = new iTextSharp.text.Paragraph($"Grand Total: R {claims.Sum(c => c.Total):F2}", headerFont);
                totalParagraph.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                document.Add(totalParagraph);

                // Add footer
                document.Add(new iTextSharp.text.Paragraph(" ")); // Empty line for spacing
                var footer = new iTextSharp.text.Paragraph("This is an automatically generated report from CMCS - University Claims Management System", normalFont);
                footer.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                document.Add(footer);

                document.Close();

                return memoryStream.ToArray();
            }
        }
    }
        
    }
