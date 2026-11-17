using System.Security.Claims;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.Application.Security;

/// <summary>Reads the claims of Hopaut access tokens; shared by <see cref="ICurrentUser"/> and the authorization policies.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>The caller's user id, or null when the principal is anonymous or has no valid user id claim.</summary>
    public static UserId? GetUserId(this ClaimsPrincipal principal) =>
        principal.Identity?.IsAuthenticated == true
        && UserId.TryParse(principal.FindFirst(HopautClaimTypes.UserId)?.Value, out var id)
            ? id
            : null;

    /// <summary>True for an authenticated caller in one of the <see cref="HopautRoles.Administrators"/> roles.</summary>
    public static bool IsAdministrator(this ClaimsPrincipal principal) =>
        principal.Identity?.IsAuthenticated == true && HopautRoles.Administrators.Any(principal.IsInRole);
}
