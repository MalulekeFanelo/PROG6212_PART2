// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using CMCS_MVC_Prototype.Services;
using CMCS_MVC_Prototype.Models;

namespace CMCS_MVC_Prototype.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Auth/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            try
            {
                var user = await _authService.Authenticate(email, password);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FullName);

                    TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return user.Role switch
                    {
                        "HR" => RedirectToAction("Index", "HR"),
                        "Coordinator" => RedirectToAction("Index", "Coordinator"),
                        "Manager" => RedirectToAction("Index", "Manager"),
                        _ => RedirectToAction("Index", "Claims")
                    };
                }

                TempData["ErrorMessage"] = "Invalid email or password";
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Login error: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully";
            return RedirectToAction("Index", "Home");
        }
    }
}