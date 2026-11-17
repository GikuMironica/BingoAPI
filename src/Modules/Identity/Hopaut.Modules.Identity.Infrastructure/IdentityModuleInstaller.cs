using FluentValidation;
using Hopaut.Modules.Identity.Application;
using Hopaut.Modules.Identity.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Hopaut.Modules.Identity.Infrastructure;

/// <summary>Registers the Identity module's services, including JWT bearer authentication; handlers and the unit-of-work behavior are registered by the Host's AddMediator.</summary>
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

        // JWT settings (validated at startup: the Host refuses to start without a signing key) and the one definition of a
        // valid token, shared by bearer authentication and the refresh flow (F-058).
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton(provider => JwtTokenValidation.Parameters(provider.GetRequiredService<IOptions<JwtSettings>>().Value));

        // AddIdentity made its cookie the default scheme; the API authenticates, challenges and forbids with bearer tokens.
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<TokenValidationParameters>((bearer, parameters) => bearer.TokenValidationParameters = parameters);

        // Repositories
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Token generator
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Validators; handlers and the unit-of-work behavior are registered by the Host's AddMediator.
        services.AddValidatorsFromAssemblyContaining<IIdentityCommand>(includeInternalTypes: true);

        return services;
    }
}
