using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Hopaut.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real Host against a disposable PostGIS container and applies every module migration on startup.
/// Needs a Docker-compatible endpoint (see tests/README.md).
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
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("JwtSettings:Secret", TestJwt.Secret);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}

/// <summary>One Host and one database for all integration test classes.</summary>
[CollectionDefinition(Name)]
public sealed class HostCollection : ICollectionFixture<HopautApiFactory>
{
    public const string Name = "Host";
}
