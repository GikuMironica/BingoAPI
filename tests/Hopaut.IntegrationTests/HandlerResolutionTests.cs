using System.Reflection;
using AwesomeAssertions.Execution;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.IntegrationTests;

/// <summary>
/// The Testing environment does not validate the container on build, so this proves that every command, query and
/// domain-event handler can be constructed from the Host's registrations, and that the source-generated mediator knows
/// every message and domain event: one it never saw (an assembly missing from AddMediator) makes Send/Publish throw.
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

    private static readonly Type[] HandlerInterfaces = [typeof(ICommandHandler<,>), typeof(IQueryHandler<,>), typeof(INotificationHandler<>)];

    [Fact]
    public void GivenTheHost_WhenEveryHandlerIsResolved_ThenOnlyKnownDefectsFail()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var handlers = ModuleTypes()
            .SelectMany(type => type.GetInterfaces().Where(IsHandlerInterface).Select(service => (Type: type, Service: service)))
            .ToList();

        // Act
        var failures = handlers
            .Select(handler => (Handler: handler.Type.Name, Error: ResolutionError(scope.ServiceProvider, handler.Service)))
            .Where(result => result.Error is not null)
            .ToList();

        // Assert
        using (new AssertionScope())
        {
            handlers.Should().HaveCountGreaterThan(40, "the Host registers every module's request and domain-event handlers");
            handlers.Should().Contain(h => h.Service.GetGenericTypeDefinition() == typeof(INotificationHandler<>),
                "the Infrastructure domain-event handlers are registered too (F-057)");
            failures.Select(f => f.Handler).Should().BeEquivalentTo(
                KnownUnresolvable.Keys,
                "only baselined handlers may fail to resolve; failures: {0}",
                string.Join(Environment.NewLine, failures.Select(f => $"{f.Handler}: {f.Error}")));
        }
    }

    [Fact]
    public void GivenTheGeneratedMediator_WhenItsOverloadsAreListed_ThenEveryMessageAndDomainEventIsKnown()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var mediatorType = scope.ServiceProvider.GetRequiredService<IMediator>().GetType();
        var sendable = FirstParameterTypes(mediatorType, nameof(IMediator.Send));
        var publishable = FirstParameterTypes(mediatorType, nameof(IMediator.Publish));
        var moduleTypes = ModuleTypes().ToList();

        // Act
        var messages = moduleTypes.Where(t => typeof(IBaseCommand).IsAssignableFrom(t) || typeof(IBaseQuery).IsAssignableFrom(t)).ToList();
        var domainEvents = moduleTypes.Where(t => typeof(IDomainEvent).IsAssignableFrom(t)).ToList();

        // Assert
        using (new AssertionScope())
        {
            messages.Should().HaveCountGreaterThan(30);
            domainEvents.Should().NotBeEmpty();
            messages.Where(t => !sendable.Contains(t)).Should().BeEmpty("every command and query must be sendable");
            domainEvents.Where(t => !publishable.Contains(t)).Should().BeEmpty("every domain event must be publishable before the save");
        }
    }

    [Fact]
    public void GivenTheHost_WhenThePipelineOfEveryMessageIsResolved_ThenEveryBehaviorConstructs()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var pipelines = ModuleTypes()
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICommand<>) || i.GetGenericTypeDefinition() == typeof(IQuery<>)))
                .Select(i => typeof(IEnumerable<>).MakeGenericType(typeof(IPipelineBehavior<,>).MakeGenericType(type, i.GenericTypeArguments[0]))))
            .ToList();

        // Act
        var failures = pipelines
            .Select(service => (Pipeline: service.GenericTypeArguments[0].GenericTypeArguments[0].Name, Error: ResolutionError(scope.ServiceProvider, service)))
            .Where(result => result.Error is not null)
            .ToList();

        // Assert
        using (new AssertionScope())
        {
            pipelines.Should().HaveCountGreaterThan(30);
            failures.Should().BeEmpty("every behavior, including each module's unit of work, must resolve; failures: {0}",
                string.Join(Environment.NewLine, failures.Select(f => $"{f.Pipeline}: {f.Error}")));
        }
    }

    private static IEnumerable<Type> ModuleTypes() =>
        Directory.GetFiles(AppContext.BaseDirectory, "Hopaut.Modules.*.dll")
            .Select(file => Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(file))))
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

    private static HashSet<Type> FirstParameterTypes(Type type, string methodName) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.Name == methodName && method.GetParameters().Length > 0)
            .Select(method => method.GetParameters()[0].ParameterType)
            .ToHashSet();

    private static bool IsHandlerInterface(Type service) =>
        service.IsGenericType && HandlerInterfaces.Contains(service.GetGenericTypeDefinition());

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
