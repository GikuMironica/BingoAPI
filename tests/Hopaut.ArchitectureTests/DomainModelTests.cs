using Hopaut.ArchitectureTests.Catalog;
using Hopaut.ArchitectureTests.Rules;

namespace Hopaut.ArchitectureTests;

public sealed class DomainModelTests
{
    private static IEnumerable<Type> ModuleDomainTypes => DomainModelRules.TypesOf(SolutionAssemblies.OfLayer(Layer.Domain));

    [Fact]
    public void Domain_types_have_no_public_setters() =>
        Ratchet.Verify(DomainModelRules.PublicSetter, DomainModelRules.PublicSetters(ModuleDomainTypes));

    [Fact]
    public void Domain_types_expose_no_mutable_collections() =>
        Ratchet.Verify(DomainModelRules.MutableCollection, DomainModelRules.PublicMutableCollections(ModuleDomainTypes));

    [Fact]
    public void Only_aggregates_raise_domain_events_and_only_through_a_protected_method() =>
        Ratchet.Verify(
            DomainModelRules.EventRaiser,
            DomainModelRules.PublicEventRaisers(
                DomainModelRules.TypesOf(SolutionAssemblies.OfLayer(Layer.SharedKernel).Concat(SolutionAssemblies.OfLayer(Layer.Domain)))));

    [Fact]
    public void Every_type_lives_under_its_assembly_root_namespace() =>
        Ratchet.Verify(
            DomainModelRules.NamespaceMatchesAssembly,
            SolutionAssemblies.All.SelectMany(DomainModelRules.NamespacesMatchAssembly));
}
