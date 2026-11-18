using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Hopaut.ArchitectureTests.Catalog;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.ArchitectureTests.Rules;

/// <summary>Messaging rules of the messaging-integration skill §3 and §11 (plan Step 7.6).</summary>
public static class MessagingRules
{
    public const string NoMediatR = "ARCH-MSG-NO-MEDIATR";
    public const string ResultResponse = "ARCH-MSG-RESULT";
    public const string ModuleMarker = "ARCH-MSG-MODULE-MARKER";
    public const string PublicTypes = "ARCH-MSG-PUBLIC";
    public const string GeneratorInHostOnly = "ARCH-MSG-GENERATOR-HOST";

    public const string HostProject = "Hopaut.Api.Host";

    private static readonly Type[] HandlerDefinitions =
    [
        typeof(ICommandHandler<,>), typeof(ICommandHandler<>), typeof(IQueryHandler<,>),
        typeof(IRequestHandler<,>), typeof(IRequestHandler<>), typeof(INotificationHandler<>),
    ];

    private static readonly Type[] ResponseDefinitions = [typeof(ICommand<>), typeof(IQuery<>), typeof(IRequest<>)];

    /// <summary>MediatR 13+ is commercially licensed; Mediator replaced it (decision D-MSG-1).</summary>
    public static IEnumerable<Violation> MediatRReferences(IEnumerable<SolutionAssembly> assemblies) =>
        from subject in assemblies
        from reference in subject.Assembly.GetReferencedAssemblies()
        where reference.Name is "MediatR" || reference.Name?.StartsWith("MediatR.", StringComparison.Ordinal) == true
        select new Violation($"{NoMediatR}: {subject.Name} -> {reference.Name}", string.Empty);

    /// <summary>Commands and queries return <see cref="Result"/> or <see cref="Result{TValue}"/>; the constrained behaviors skip anything else.</summary>
    public static IEnumerable<Violation> NonResultResponses(IEnumerable<Type> types) =>
        from type in types.Where(IsConcreteUserType)
        from contract in type.GetInterfaces()
        where contract.IsGenericType && ResponseDefinitions.Contains(contract.GetGenericTypeDefinition())
        let response = contract.GenericTypeArguments[0]
        where response != typeof(Result) && !(response.IsGenericType && response.GetGenericTypeDefinition() == typeof(Result<>))
        select new Violation($"{ResultResponse}: {type.FullName}", response.Name);

    /// <summary>Every command implements exactly one marker of its own module, which selects that module's unit of work.</summary>
    public static IEnumerable<Violation> CommandMarkers(string module, IEnumerable<Type> types)
    {
        var expected = $"I{module}Command";
        foreach (var command in types.Where(t => IsConcreteUserType(t) && typeof(IBaseCommand).IsAssignableFrom(t)))
        {
            var markers = command.GetInterfaces()
                .Where(i => !i.IsGenericType && i.Assembly != typeof(IBaseCommand).Assembly && typeof(IBaseCommand).IsAssignableFrom(i))
                .Select(i => i.Name)
                .ToList();

            if (markers is not [var marker] || marker != expected)
            {
                yield return new Violation($"{ModuleMarker}: {command.FullName}", $"expected only {expected}, found [{string.Join(", ", markers)}]");
            }
        }
    }

    /// <summary>The Host's source generator can only register messages and handlers it can see.</summary>
    public static IEnumerable<Violation> NonPublicMessagingTypes(IEnumerable<Type> types) =>
        from type in types.Where(IsConcreteUserType)
        where IsMessage(type) || IsHandler(type)
        where !type.IsVisible
        select new Violation($"{PublicTypes}: {type.FullName}", string.Empty);

    /// <summary>One generator per application: Mediator.SourceGenerator in several projects breaks the generated code.</summary>
    public static IEnumerable<Violation> GeneratorReferences(IEnumerable<string> projectFiles) =>
        from file in projectFiles
        let project = Path.GetFileNameWithoutExtension(file)
        where project != HostProject
        where XDocument.Load(file).Descendants().Any(e => e.Name.LocalName == "PackageReference"
                                                          && (string?)e.Attribute("Include") == "Mediator.SourceGenerator")
        select new Violation($"{GeneratorInHostOnly}: {project}", string.Empty);

    private static bool IsMessage(Type type) => typeof(IMessage).IsAssignableFrom(type) || typeof(INotification).IsAssignableFrom(type);

    private static bool IsHandler(Type type) =>
        type.GetInterfaces().Any(i => i.IsGenericType && HandlerDefinitions.Contains(i.GetGenericTypeDefinition()));

    private static bool IsConcreteUserType(Type type) =>
        type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
        && !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
        && !type.Name.Contains('<');
}
