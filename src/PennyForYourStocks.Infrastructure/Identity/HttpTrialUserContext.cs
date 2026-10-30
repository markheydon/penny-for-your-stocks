using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PennyForYourStocks.Application.Abstractions;

namespace PennyForYourStocks.Infrastructure.Identity;

internal sealed class HttpTrialUserContext(IHttpContextAccessor httpContextAccessor) : ITrialUserContext
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
