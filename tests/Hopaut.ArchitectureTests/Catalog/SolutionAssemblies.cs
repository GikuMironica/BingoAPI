using System.Reflection;

namespace Hopaut.ArchitectureTests.Catalog;

public enum Layer
{
    SharedKernel,
    IntegrationEvents,
    BuildingBlocksApplication,
    BuildingBlocksInfrastructure,
    BuildingBlocksApi,
    Domain,
    Application,
    Contracts,
    Infrastructure,
    Api,
}

public sealed record SolutionAssembly(Assembly Assembly, string Name, Layer Layer, string? Module)
{
    public override string ToString() => Name;
}

/// <summary>Every production assembly copied next to the test assembly (all src projects are referenced by glob).</summary>
public static class SolutionAssemblies
{
    public const string ModulePrefix = "Hopaut.Modules.";

    public static readonly Layer[] ModuleLayers = [Layer.Domain, Layer.Application, Layer.Contracts, Layer.Infrastructure, Layer.Api];

    private static readonly Lazy<IReadOnlyList<SolutionAssembly>> Loaded = new(Load);

    public static IReadOnlyList<SolutionAssembly> All => Loaded.Value;

    public static IReadOnlyList<string> ModuleNames =>
        All.Where(a => a.Module is not null).Select(a => a.Module!).Distinct().Order(StringComparer.Ordinal).ToList();

    public static IEnumerable<SolutionAssembly> OfLayer(Layer layer) => All.Where(a => a.Layer == layer);

    public static SolutionAssembly Single(Layer layer) => All.Single(a => a.Layer == layer);

    public static (Layer Layer, string? Module)? Classify(string assemblyName) => assemblyName switch
    {
        "Hopaut.SharedKernel" => (Layer.SharedKernel, null),
        "Hopaut.IntegrationEvents" => (Layer.IntegrationEvents, null),
        "Hopaut.BuildingBlocks.Application" => (Layer.BuildingBlocksApplication, null),
        "Hopaut.BuildingBlocks.Infrastructure" => (Layer.BuildingBlocksInfrastructure, null),
        "Hopaut.BuildingBlocks.Api" => (Layer.BuildingBlocksApi, null),
        _ when assemblyName.StartsWith(ModulePrefix, StringComparison.Ordinal) => ClassifyModule(assemblyName),
        _ => null,
    };

    public static string ModuleAssemblyName(string module, Layer layer) => $"{ModulePrefix}{module}.{layer}";

    private static (Layer Layer, string? Module)? ClassifyModule(string assemblyName)
    {
        var parts = assemblyName[ModulePrefix.Length..].Split('.');
        if (parts.Length != 2 || !Enum.TryParse<Layer>(parts[1], out var layer) || !ModuleLayers.Contains(layer))
        {
            return null;
        }

        return (layer, parts[0]);
    }

    private static IReadOnlyList<SolutionAssembly> Load()
    {
        var self = typeof(SolutionAssemblies).Assembly.GetName().Name;
        var result = new List<SolutionAssembly>();
        foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "Hopaut.*.dll"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (name == self || Classify(name) is not { } classified)
            {
                continue;
            }

            result.Add(new SolutionAssembly(Assembly.Load(new AssemblyName(name)), name, classified.Layer, classified.Module));
        }

        return result.OrderBy(a => a.Name, StringComparer.Ordinal).ToList();
    }
}
