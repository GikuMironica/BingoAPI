using System.Security.Claims;
using Hopaut.BuildingBlocks.Application.Security;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests.Infrastructure;

/// <summary>Sends messages straight to the mediator as a given caller, the way an authenticated request would.</summary>
public static class TestCaller
{
    /// <summary>An authenticated principal carrying the same claims as a Hopaut access token.</summary>
    public static ClaimsPrincipal For(string userId, params string[] roles) =>
        new(new ClaimsIdentity(
            [new Claim(HopautClaimTypes.UserId, userId), .. roles.Select(role => new Claim(ClaimTypes.Role, role))],
            authenticationType: "Test"));

    /// <summary>
    /// Sends <paramref name="command"/> in a fresh scope with <paramref name="caller"/> as the request principal read by
    /// ICurrentUser and IResourceAuthorizer; null sends it without a request, like background work.
    /// </summary>
    public static async Task<TResponse> SendAsync<TResponse>(
        IServiceProvider root, ClaimsPrincipal? caller, ICommand<TResponse> command, CancellationToken cancellationToken)
    {
        await using var scope = root.CreateAsyncScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = caller is null ? null : new DefaultHttpContext { User = caller, RequestServices = scope.ServiceProvider };
        try
        {
            return await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, cancellationToken);
        }
        finally
        {
            accessor.HttpContext = null;
        }
    }
}
