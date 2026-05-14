using Hopaut.Modules.Users.Application;
using Hopaut.Modules.Users.Application.Queries.GetUserProfile;
using Hopaut.Modules.Users.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Users.Api;

public static class UsersModuleInstaller
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<UsersModuleDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", UsersModuleDbContext.SchemaName)));

        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<GetUserProfileQuery>());

        return services;
    }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new UsersModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
