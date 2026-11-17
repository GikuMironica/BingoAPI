using Hopaut.ArchitectureTests.Catalog;
using Hopaut.ArchitectureTests.Rules;

namespace Hopaut.ArchitectureTests;

/// <summary>One test per row of the layer table (clean-architecture-ddd skill §1), checked against the ratchet baseline.</summary>
public sealed class LayerDependencyTests
{
    public static TheoryData<string> AllLayers() => new(Enum.GetNames<Layer>());

    [Theory]
    [MemberData(nameof(AllLayers))]
    public void Layer_only_depends_on_what_the_layer_table_allows(string layerName)
    {
        var layer = Enum.Parse<Layer>(layerName);

        var violations = SolutionAssemblies.OfLayer(layer).SelectMany(LayerRules.Evaluate);

        Ratchet.Verify(LayerRules.RuleFor(layer), violations);
    }
}
