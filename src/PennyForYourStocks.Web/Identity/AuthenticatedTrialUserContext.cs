using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using PennyForYourStocks.Application.Abstractions;

namespace PennyForYourStocks.Web.Identity;

/// <summary>
/// Resolves the trial user from HTTP requests and from Blazor interactive circuits
/// (where <see cref="IHttpContextAccessor.HttpContext"/> is unavailable).
/// </summary>
public sealed class AuthenticatedTrialUserContext(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider authenticationStateProvider) : ITrialUserContext
{
    public string? UserId
    {
        get
        {
            var fromHttp = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(fromHttp))
            {
                return fromHttp;
            }

            if (httpContextAccessor.HttpContext is not null)
            {
                return null;
            }

            try
            {
                var state = authenticationStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
                return state.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
