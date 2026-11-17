using FluentValidation;
using Hopaut.Modules.Identity.Application;
using Hopaut.Modules.Identity.Domain;
using Hopaut.Modules.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Identity.Api;

public static class IdentityModuleInstaller
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddIdentityPersistence(connectionString);

        services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<IdentityModuleDbContext>()
            .AddDefaultTokenProviders();

        // JWT settings
        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);
        services.AddSingleton(jwtSettings);

        // Repositories
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Token generator
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Validators; handlers and the unit-of-work behavior are registered by the Host's AddMediator.
        services.AddValidatorsFromAssemblyContaining<IIdentityCommand>(includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new IdentityModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
