using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EtherApp.Middleware
{
    public class RoleBasedRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleBasedRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
        {
            // Only check on the home route for authenticated users
            if (context.Request.Path == "/" ||
                context.Request.Path == "/Home" ||
                context.Request.Path == "/Home/Index")
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    // Get the user
                    var user = await userManager.GetUserAsync(context.User);
                    if (user != null && await userManager.IsInRoleAsync(user, AppRoles.Admin))
                    {
                        // Redirect admin to admin dashboard
                        context.Response.Redirect("/Admin/Index");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method to make it easier to add the middleware
    public static class RoleBasedRedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleBasedRedirect(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RoleBasedRedirectMiddleware>();
        }
    }
}