

// Controllers/ClaimsController.cs
using CMCS_MVC_Prototype.Attributes;
using CMCS_MVC_Prototype.Models;
using CMCS_MVC_Prototype.Services;
using Microsoft.AspNetCore.Mvc;

namespace CMCS_MVC_Prototype.Controllers
{
    [Authorize("Lecturer", "HR", "Coordinator", "Manager")]
    public class ClaimsController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly IUserService _userService;

        public ClaimsController(IClaimService claimService, IUserService userService)
        {
            _claimService = claimService;
            _userService = userService;
        }

        // GET: Claims/Index (My Claims)
        public async Task<IActionResult> Index()
        {
            var currentUser = HttpContext.Items["User"] as User;
            List<Claim> claims;

            if (currentUser.Role == "HR")
            {
                claims = await _claimService.GetAllClaimsAsync();
            }
            else
            {
                claims = await _claimService.GetClaimsByLecturerIdAsync(currentUser.LecturerId);
            }

            return View(claims);
        }

        // GET: Claims/Create (Submit Claim)
        public async Task<IActionResult> Create()
        {
            var currentUser = HttpContext.Items["User"] as User;
            if (currentUser.Role != "Lecturer")
            {
                TempData["ErrorMessage"] = "Only lecturers can submit claims";
                return RedirectToAction(nameof(Index));
            }

            var claim = new Claim
            {
                LecturerName = currentUser.FullName,
                LecturerId = currentUser.LecturerId,
                HourlyRate = currentUser.HourlyRate
            };

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile document)
        {
            var currentUser = HttpContext.Items["User"] as User;

            Console.WriteLine("=== CLAIM CREATE DEBUG START ===");
            Console.WriteLine($"User: {currentUser?.FullName}, Role: {currentUser?.Role}");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Log ModelState errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Errors:");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($"  {key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            // Validate hours worked (max 180 hours per month)
            if (claim.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked cannot exceed 180 hours per month");
                Console.WriteLine("Hours worked validation failed: Exceeds 180 hours");
            }

            Console.WriteLine($"HoursWorked: {claim.HoursWorked}");
            Console.WriteLine($"Month: {claim.Month}");
            Console.WriteLine($"Notes: {claim.Notes}");
            Console.WriteLine($"Document: {(document != null ? document.FileName : "NULL")}");

            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine("ModelState is valid - proceeding with claim creation");

                    // Auto-populate from user profile (override any user input)
                    claim.LecturerName = currentUser.FullName;
                    claim.LecturerId = currentUser.LecturerId;
                    claim.HourlyRate = currentUser.HourlyRate; // This comes from HR
                    claim.Status = "Pending";
                    claim.Submitted = DateTime.Now;

                    Console.WriteLine($"Auto-populated - Name: {claim.LecturerName}, ID: {claim.LecturerId}, Rate: {claim.HourlyRate}");

                    // Handle file upload
                    if (document != null && document.Length > 0)
                    {
                        Console.WriteLine("Starting file upload process...");

                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Documents");
                        Console.WriteLine($"Uploads folder: {uploadsFolder}");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Console.WriteLine("Creating Documents directory...");
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + document.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        Console.WriteLine($"File will be saved as: {uniqueFileName}");
                        Console.WriteLine($"Full path: {filePath}");

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await document.CopyToAsync(stream);
                        }

                        Console.WriteLine("File saved successfully!");
                        claim.DocumentPath = uniqueFileName;
                    }
                    else
                    {
                        Console.WriteLine("No file was uploaded or file is empty");
                        claim.DocumentPath = "";
                    }

                    Console.WriteLine("Saving claim to database...");
                    await _claimService.CreateClaimAsync(claim);
                    Console.WriteLine("Claim saved to database successfully!");

                    TempData["SuccessMessage"] = "Claim submitted successfully!";
                    Console.WriteLine("=== CLAIM CREATE DEBUG END - SUCCESS ===");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    TempData["ErrorMessage"] = $"Error submitting claim: {ex.Message}";
                    Console.WriteLine("=== CLAIM CREATE DEBUG END - EXCEPTION ===");
                }
            }
            else
            {
                Console.WriteLine("ModelState is invalid - returning to view with errors");
                Console.WriteLine("=== CLAIM CREATE DEBUG END - VALIDATION FAILED ===");
            }

            return View(claim);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _claimService.DeleteClaimAsync(id);
                TempData["SuccessMessage"] = "Claim deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting claim: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}