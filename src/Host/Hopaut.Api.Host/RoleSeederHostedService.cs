using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hopaut.Api.Host;

/// <summary>
/// Idempotently seeds the default Identity roles (Admin, User, SuperAdmin) at startup.
/// Skipped when the <c>--no-seed</c> command-line argument is present.
/// </summary>
public sealed class RoleSeederHostedService : IHostedLifecycleService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _env;
    private readonly ILogger<RoleSeederHostedService> _logger;
    private readonly string[] _requiredRoles = ["Admin", "User", "SuperAdmin"];

    public RoleSeederHostedService(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment env,
        ILogger<RoleSeederHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _env = env;
        _logger = logger;
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Allow skipping via --no-seed argument
        if (Environment.GetCommandLineArgs().Contains("--no-seed"))
        {
            _logger.LogInformation("Role seeding skipped (--no-seed)");
            return;
        }

        _logger.LogInformation("Seeding identity roles...");

        using var scope = _scopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in _requiredRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation("Created role: {Role}", role);
            }
        }

        _logger.LogInformation("Role seeding complete");
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
