// Middleware/SessionAuthMiddleware.cs
using CMCS_MVC_Prototype.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CMCS_MVC_Prototype.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            // Check if user is logged in
            var userId = context.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = await authService.GetUserById(userId.Value);
                if (user != null)
                {
                    context.Items["User"] = user;
                    context.Items["UserRole"] = user.Role;
                }
            }

            await _next(context);
        }
    }
}

