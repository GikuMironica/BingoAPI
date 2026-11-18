using System.Security.Claims;
using Hopaut.BuildingBlocks.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Hopaut.BuildingBlocks.Api.Security;

/// <summary>
/// <see cref="IResourceAuthorizer"/> backed by ASP.NET Core authorization: evaluates the named policy for the principal of the
/// current request, or for an anonymous principal when there is no request. An unknown policy name throws.
/// </summary>
public sealed class AuthorizationServiceResourceAuthorizer(IAuthorizationService authorization, IHttpContextAccessor accessor)
    : IResourceAuthorizer
{
    public async Task<bool> IsAuthorizedAsync(object resource, string policy, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentException.ThrowIfNullOrWhiteSpace(policy);
        cancellationToken.ThrowIfCancellationRequested();

        var caller = accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        var result = await authorization.AuthorizeAsync(caller, resource, policy);
        return result.Succeeded;
    }
}
