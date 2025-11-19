using Microsoft.AspNetCore.Mvc;
using CMCS_MVC_Prototype.Services;
using CMCS_MVC_Prototype.Attributes;
using System.Threading.Tasks;

namespace CMCS_MVC_Prototype.Controllers
{
    [Authorize("Coordinator", "HR")] // Only Coordinators and HR can access
    public class CoordinatorController : Controller
    {
        private readonly IClaimService _claimService;

        public CoordinatorController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        // GET: /Coordinator
        public async Task<IActionResult> Index()
        {
            var pendingClaims = await _claimService.GetClaimsByStatusAsync("Pending");
            return View(pendingClaims);
        }

        // POST: /Coordinator/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            await _claimService.UpdateClaimStatusAsync(id, "Approved by Coordinator", "Coordinator");
            TempData["SuccessMessage"] = "Claim approved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Coordinator/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            await _claimService.UpdateClaimStatusAsync(id, "Rejected by Coordinator", "Coordinator");
            TempData["SuccessMessage"] = "Claim rejected!";
            return RedirectToAction(nameof(Index));
        }
    }
}