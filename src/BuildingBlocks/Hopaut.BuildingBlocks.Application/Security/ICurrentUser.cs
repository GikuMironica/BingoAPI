using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.Application.Security;

/// <summary>The authenticated caller. Handlers use this port instead of HttpContext or user ids sent by the client.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>The caller's user id. Throws when the request is anonymous: guard such endpoints with RequireAuthorization.</summary>
    UserId Id { get; }

    bool IsInRole(string role);
}

/// <summary>Claim types issued in Hopaut access tokens.</summary>
public static class HopautClaimTypes
{
    /// <summary>The user id claim emitted by the Identity module and read by <see cref="ICurrentUser"/>.</summary>
    public const string UserId = "id";
}
