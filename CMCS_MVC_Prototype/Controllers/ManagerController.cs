using Microsoft.AspNetCore.Mvc;
using CMCS_MVC_Prototype.Services;
using CMCS_MVC_Prototype.Attributes;
using System.Threading.Tasks;

namespace CMCS_MVC_Prototype.Controllers
{
    [Authorize("Manager", "HR")] // Only Managers and HR can access
    public class ManagerController : Controller
    {
        private readonly IClaimService _claimService;

        public ManagerController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        // GET: /Manager
        public async Task<IActionResult> Index()
        {
            var toReview = await _claimService.GetClaimsByStatusAsync("Approved by Coordinator");
            return View(toReview);
        }

        // POST: /Manager/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            await _claimService.UpdateClaimStatusAsync(id, "Approved by Manager", "Manager");
            TempData["SuccessMessage"] = "Claim approved and paid!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Manager/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            await _claimService.UpdateClaimStatusAsync(id, "Rejected by Manager", "Manager");
            TempData["SuccessMessage"] = "Claim rejected!";
            return RedirectToAction(nameof(Index));
        }
    }
}