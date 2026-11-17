using Hopaut.ArchitectureTests.Catalog;
using Hopaut.ArchitectureTests.Rules;

namespace Hopaut.ArchitectureTests;

/// <summary>Mediator messaging conventions (messaging-integration skill §3, §11), checked against the ratchet baseline.</summary>
public sealed class MessagingTests
{
    private static IEnumerable<Type> AllTypes => DomainModelRules.TypesOf(SolutionAssemblies.All);

    [Fact]
    public void No_assembly_references_MediatR() =>
        Ratchet.Verify(MessagingRules.NoMediatR, MessagingRules.MediatRReferences(SolutionAssemblies.All));

    [Fact]
    public void Every_command_and_query_returns_a_Result() =>
        Ratchet.Verify(MessagingRules.ResultResponse, MessagingRules.NonResultResponses(AllTypes));

    [Fact]
    public void Every_command_implements_exactly_the_marker_of_its_own_module() =>
        Ratchet.Verify(
            MessagingRules.ModuleMarker,
            SolutionAssemblies.All
                .Where(a => a.Module is not null)
                .SelectMany(a => MessagingRules.CommandMarkers(a.Module!, a.Assembly.GetTypes())));

    [Fact]
    public void Messages_and_handlers_are_public_so_the_Host_generator_can_register_them() =>
        Ratchet.Verify(MessagingRules.PublicTypes, MessagingRules.NonPublicMessagingTypes(AllTypes));

    [Fact]
    public void Only_the_Host_references_the_Mediator_source_generator()
    {
        var projects = Directory.GetFiles(Path.Combine(SuiteIntegrityTests.RepositoryRoot(), "src"), "*.csproj", SearchOption.AllDirectories);

        Assert.Contains(projects, p => Path.GetFileNameWithoutExtension(p) == MessagingRules.HostProject);
        Ratchet.Verify(MessagingRules.GeneratorInHostOnly, MessagingRules.GeneratorReferences(projects));
    }
}
