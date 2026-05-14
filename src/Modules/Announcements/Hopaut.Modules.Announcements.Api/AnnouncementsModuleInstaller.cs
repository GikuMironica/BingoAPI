using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;
using Hopaut.Modules.Announcements.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Announcements.Api;

public static class AnnouncementsModuleInstaller
{
    public static IServiceCollection AddAnnouncementsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AnnouncementsModuleDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", AnnouncementsModuleDbContext.SchemaName)));

        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateAnnouncementCommand>());

        return services;
    }

    public static IEndpointRouteBuilder MapAnnouncementsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new AnnouncementsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
