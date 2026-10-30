using Microsoft.AspNetCore.Identity;
using PennyForYourStocks.Infrastructure.Identity;

namespace PennyForYourStocks.Web.Components.Account;

/// <summary>
/// Signs out when the auth cookie references a user that no longer exists (e.g. after Aspire SQLite reset).
/// </summary>
internal sealed class StaleIdentitySessionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && await userManager.GetUserAsync(context.User) is null)
        {
            await signInManager.SignOutAsync();
        }

        await next(context);
    }
}

internal static class StaleIdentitySessionMiddlewareExtensions
{
    public static IApplicationBuilder UseStaleIdentitySessionCleanup(this IApplicationBuilder app) =>
        app.UseMiddleware<StaleIdentitySessionMiddleware>();
}
