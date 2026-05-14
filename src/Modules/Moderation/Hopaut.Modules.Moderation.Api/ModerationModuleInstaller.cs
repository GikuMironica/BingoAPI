using Hopaut.Modules.Moderation.Application;
using Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;
using Hopaut.Modules.Moderation.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Moderation.Api;

public static class ModerationModuleInstaller
{
    public static IServiceCollection AddModerationModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ModerationModuleDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", ModerationModuleDbContext.SchemaName)));

        services.AddScoped<IModerationRepository, ModerationRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreatePostReportCommand>());

        return services;
    }

    public static IEndpointRouteBuilder MapModerationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new ModerationModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
