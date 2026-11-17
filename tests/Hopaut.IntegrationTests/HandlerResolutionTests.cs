using AwesomeAssertions.Execution;
using Hopaut.IntegrationTests.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>
/// The Testing environment does not validate the container on build, so this proves that every request handler can be
/// constructed from the Host's registrations (for example the <see cref="TimeProvider"/> handlers take "now" from).
/// </summary>
[Collection(HostCollection.Name)]
public sealed class HandlerResolutionTests(HopautApiFactory factory)
{
    // Known defects, each tied to an open finding in TODO.md. Entries may only be removed (ratchet, decision D-002):
    // a new unresolvable handler fails the test, and so does an entry whose handler resolves again.
    private static readonly IReadOnlyDictionary<string, string> KnownUnresolvable = new Dictionary<string, string>
    {
        ["RefreshTokenCommandHandler"] = "F-058", // TokenValidationParameters is not registered (plan Step 7.7)
    };

    [Fact]
    public void GivenTheHost_WhenEveryRequestHandlerIsResolved_ThenOnlyKnownDefectsFail()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.GetName().Name?.StartsWith("Hopaut.Modules.", StringComparison.Ordinal) == true)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.GetInterfaces().Any(IsRequestHandler))
            .ToList();

        // Act
        var failures = handlerTypes
            .Select(type => (Handler: type.Name, Error: ResolutionError(scope.ServiceProvider, type.GetInterfaces().First(IsRequestHandler))))
            .Where(result => result.Error is not null)
            .ToList();

        // Assert
        using (new AssertionScope())
        {
            handlerTypes.Should().HaveCountGreaterThan(20, "the Host loads every module's Application assembly");
            failures.Select(f => f.Handler).Should().BeEquivalentTo(
                KnownUnresolvable.Keys,
                "only baselined handlers may fail to resolve; failures: {0}",
                string.Join(Environment.NewLine, failures.Select(f => $"{f.Handler}: {f.Error}")));
        }
    }

    private static bool IsRequestHandler(Type service) =>
        service.IsGenericType && service.GetGenericTypeDefinition() == typeof(IRequestHandler<,>);

    private static string? ResolutionError(IServiceProvider provider, Type service)
    {
        try
        {
            return provider.GetService(service) is null ? "not registered" : null;
        }
        catch (InvalidOperationException exception)
        {
            return exception.Message;
        }
    }
}
