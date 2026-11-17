using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Hopaut.BuildingBlocks.Api.Security;

/// <summary><see cref="ICurrentUser"/> backed by the authenticated principal of the current request.</summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true && UserIdClaim is not null;

    public UserId Id => UserIdClaim is { } value && UserId.TryParse(value, out var id)
        ? id
        : throw new InvalidOperationException("The current request has no authenticated user.");

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;

    private string? UserIdClaim => Principal?.Identity?.IsAuthenticated == true
        ? Principal.FindFirst(HopautClaimTypes.UserId)?.Value
        : null;
}

public static class CurrentUserServiceCollectionExtensions
{
    public static IServiceCollection AddHopautCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        return services;
    }
}
