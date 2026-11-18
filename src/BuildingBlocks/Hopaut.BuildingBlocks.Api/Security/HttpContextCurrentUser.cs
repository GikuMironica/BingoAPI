using System.Security.Claims;
using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.BuildingBlocks.Api.Security;

/// <summary><see cref="ICurrentUser"/> backed by the authenticated principal of the current request.</summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.GetUserId() is not null;

    public UserId Id => Principal?.GetUserId()
        ?? throw new InvalidOperationException("The current request has no authenticated user.");

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;
}

public static class CurrentUserServiceCollectionExtensions
{
    public static IServiceCollection AddHopautCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        return services;
    }

    /// <summary>Registers <see cref="IResourceAuthorizer"/>; the policies it evaluates are registered by the modules.</summary>
    public static IServiceCollection AddHopautResourceAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddAuthorizationCore();
        services.AddScoped<IResourceAuthorizer, AuthorizationServiceResourceAuthorizer>();
        return services;
    }
}
