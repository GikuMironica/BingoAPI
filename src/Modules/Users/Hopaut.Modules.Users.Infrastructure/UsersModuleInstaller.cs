using FluentValidation;
using Hopaut.BuildingBlocks.Infrastructure.Authorization;
using Hopaut.Modules.Users.Application;
using Hopaut.Modules.Users.Application.Authorization;
using Hopaut.Modules.Users.Domain;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Users.Infrastructure;

/// <summary>Registers the Users module's services. Handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
public static class UsersModuleInstaller
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddUsersPersistence(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        // TODO (F-019, plan Step 8.8): IdentityUserId becomes a UserId; until then an unparsable value has no owner.
        services.AddResourceOwnerPolicy<UserProfile>(
            UserProfilePolicies.MustOwnProfile,
            profile => UserId.TryParse(profile.IdentityUserId, out var owner) ? owner : null,
            administratorsAllowed: true);

        services.AddValidatorsFromAssemblyContaining<IUsersCommand>(includeInternalTypes: true);

        return services;
    }
}
