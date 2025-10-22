

using Microsoft.AspNetCore.Mvc;
using CMCS_MVC_Prototype.Models;
using CMCS_MVC_Prototype.Services;
using System.Threading.Tasks;

namespace CMCS_MVC_Prototype.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        // GET: Claims/Index (My Claims)
        public async Task<IActionResult> Index(string lecturerId)
        {
            var claims = string.IsNullOrEmpty(lecturerId)
                ? await _claimService.GetAllClaimsAsync()
                : await _claimService.GetClaimsByLecturerIdAsync(lecturerId);

            return View(claims);
        }

        // GET: Claims/Create (Submit Claim)
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile document)
        {
            Console.WriteLine("=== FILE UPLOAD DEBUG ===");
            Console.WriteLine($"Document is null: {document == null}");

            if (document != null)
            {
                Console.WriteLine($"File name: {document.FileName}");
                Console.WriteLine($"File length: {document.Length}");
                Console.WriteLine($"Content type: {document.ContentType}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    claim.Status = "Pending";
                    claim.Submitted = DateTime.Now;

                    // Handle file upload
                    if (document != null && document.Length > 0)
                    {
                        Console.WriteLine("Starting file upload process...");

                        // 1. Define the uploads folder path
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Documents");
                        Console.WriteLine($"Uploads folder: {uploadsFolder}");

                        // 2. Create the folder if it doesn't exist
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Console.WriteLine("Creating Documents directory...");
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // 3. Generate a unique filename to prevent overwrites
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + document.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        Console.WriteLine($"File will be saved as: {uniqueFileName}");
                        Console.WriteLine($"Full path: {filePath}");

                        // 4. Save the file to the server
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await document.CopyToAsync(stream);
                        }

                        Console.WriteLine("File saved successfully!");

                        // 5. Store the filename in the database
                        claim.DocumentPath = uniqueFileName;
                        Console.WriteLine($"DocumentPath set to: {claim.DocumentPath}");
                    }
                    else
                    {
                        Console.WriteLine("No file was uploaded or file is empty");
                        claim.DocumentPath = "";
                    }

                    // Generate Lecturer ID and save claim
                    claim.GenerateLecturerId();
                    await _claimService.CreateClaimAsync(claim);

                    Console.WriteLine("Claim saved to database successfully!");

                    TempData["SuccessMessage"] = $"Claim submitted successfully! Your Lecturer ID is: {claim.LecturerId}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    TempData["ErrorMessage"] = $"Error submitting claim: {ex.Message}";
                }
            }

            return View(claim);
        }
        // POST: Claims/Delete
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