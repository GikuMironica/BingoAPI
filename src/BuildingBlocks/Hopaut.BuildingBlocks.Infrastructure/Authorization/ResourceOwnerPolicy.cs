using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hopaut.BuildingBlocks.Infrastructure.Authorization;

/// <summary>
/// Satisfied when the caller owns the resource, or is an administrator and the policy lets administrators act (plan Step 6.9).
/// Each policy holds its own instance; <see cref="ResourceOwnerAuthorizationHandler{TResource}"/> evaluates it.
/// </summary>
public sealed class ResourceOwnerRequirement<TResource>(Func<TResource, UserId?> ownerOf, bool administratorsAllowed)
    : IAuthorizationRequirement
    where TResource : class
{
    public bool AdministratorsAllowed { get; } = administratorsAllowed;

    public UserId? OwnerOf(TResource resource) => ownerOf(resource);
}

/// <summary>Evaluates every <see cref="ResourceOwnerRequirement{TResource}"/> against the caller's user id and roles.</summary>
public sealed class ResourceOwnerAuthorizationHandler<TResource>
    : AuthorizationHandler<ResourceOwnerRequirement<TResource>, TResource>
    where TResource : class
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ResourceOwnerRequirement<TResource> requirement, TResource resource)
    {
        var isOwner = context.User.GetUserId() is { } caller && requirement.OwnerOf(resource) == caller;
        if (isOwner || (requirement.AdministratorsAllowed && context.User.IsAdministrator()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class ResourceOwnerPolicyServiceCollectionExtensions
{
    /// <summary>
    /// Registers <paramref name="policy"/>: an authenticated caller who owns the resource according to <paramref name="ownerOf"/>,
    /// or an administrator when <paramref name="administratorsAllowed"/>. Registering the same policy name twice throws when the
    /// authorization options are built, so two modules cannot silently replace each other's policy.
    /// </summary>
    public static IServiceCollection AddResourceOwnerPolicy<TResource>(
        this IServiceCollection services, string policy, Func<TResource, UserId?> ownerOf, bool administratorsAllowed)
        where TResource : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policy);
        ArgumentNullException.ThrowIfNull(ownerOf);

        services.AddAuthorizationCore();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IAuthorizationHandler, ResourceOwnerAuthorizationHandler<TResource>>());
        services.Configure<AuthorizationOptions>(options =>
        {
            if (options.GetPolicy(policy) is not null)
            {
                throw new InvalidOperationException($"The authorization policy '{policy}' is registered more than once.");
            }

            options.AddPolicy(policy, builder => builder
                .RequireAuthenticatedUser()
                .AddRequirements(new ResourceOwnerRequirement<TResource>(ownerOf, administratorsAllowed)));
        });

        return services;
    }
}
