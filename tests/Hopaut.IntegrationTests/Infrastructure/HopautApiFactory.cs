using Hopaut.Api.Host;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace Hopaut.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real Host against a disposable PostGIS container. Needs a Docker-compatible endpoint (see tests/README.md).
/// </summary>
public sealed class HopautApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string PostGisImage = "postgis/postgis:17-3.5";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder(PostGisImage).Build();

    public string ConnectionString => _database.GetConnectionString();

    public async ValueTask InitializeAsync() => await _database.StartAsync(TestContext.Current.CancellationToken);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        builder.UseSetting("ConnectionStrings:HangfireConnection", ConnectionString);
        builder.ConfigureTestServices(services =>
        {
            // The Identity tables only exist once the module migrations land (plan Step 7.3); until then the seeder cannot run.
            var seeder = services.Single(d =>
                d.ServiceType == typeof(IHostedService) && !d.IsKeyedService && d.ImplementationType == typeof(RoleSeederHostedService));
            services.Remove(seeder);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}
