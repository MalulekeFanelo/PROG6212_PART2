// Attributes/AuthorizeAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CMCS_MVC_Prototype.Attributes
{
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Items["User"] as Models.User;
            var userRole = context.HttpContext.Items["UserRole"] as string;

            if (user == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(userRole))
            {
                context.Result = new ViewResult { ViewName = "AccessDenied" };
                return;
            }
        }
    }
}