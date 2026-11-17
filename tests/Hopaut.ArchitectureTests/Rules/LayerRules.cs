using Hopaut.ArchitectureTests.Catalog;
using NetArchTest.Rules;
using System.Reflection;

namespace Hopaut.ArchitectureTests.Rules;

public sealed record Violation(string Id, string Detail)
{
    public override string ToString() => string.IsNullOrEmpty(Detail) ? Id : $"{Id}  ({Detail})";
}

/// <summary>
/// One rule per row of the layer table in .github/skills/clean-architecture-ddd/SKILL.md §1.
/// A violation id has the form "<rule>: <subject assembly> -> <forbidden namespace>".
/// </summary>
public static class LayerRules
{
    public const string SharedKernel = "ARCH-LAYER-SHAREDKERNEL";
    public const string IntegrationEvents = "ARCH-LAYER-INTEGRATIONEVENTS";
    public const string BuildingBlocksApplication = "ARCH-LAYER-BB-APPLICATION";
    public const string BuildingBlocksInfrastructure = "ARCH-LAYER-BB-INFRASTRUCTURE";
    public const string BuildingBlocksApi = "ARCH-LAYER-BB-API";
    public const string Domain = "ARCH-LAYER-DOMAIN";
    public const string Application = "ARCH-LAYER-APPLICATION";
    public const string Contracts = "ARCH-LAYER-CONTRACTS";
    public const string Infrastructure = "ARCH-LAYER-INFRASTRUCTURE";
    public const string Api = "ARCH-LAYER-API";

    public const string EfCore = "Microsoft.EntityFrameworkCore";
    public const string AspNetCore = "Microsoft.AspNetCore";
    public const string Hangfire = "Hangfire";
    public const string MassTransit = "MassTransit";
    public const string DataAnnotations = "System.ComponentModel.DataAnnotations";

    private static readonly string[] Frameworks = [EfCore, AspNetCore, Hangfire, MassTransit];

    private static readonly Layer[] ImplementationLayers = [Layer.Domain, Layer.Application, Layer.Infrastructure, Layer.Api];

    public static string RuleFor(Layer layer) => layer switch
    {
        Layer.SharedKernel => SharedKernel,
        Layer.IntegrationEvents => IntegrationEvents,
        Layer.BuildingBlocksApplication => BuildingBlocksApplication,
        Layer.BuildingBlocksInfrastructure => BuildingBlocksInfrastructure,
        Layer.BuildingBlocksApi => BuildingBlocksApi,
        Layer.Domain => Domain,
        Layer.Application => Application,
        Layer.Contracts => Contracts,
        Layer.Infrastructure => Infrastructure,
        Layer.Api => Api,
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null),
    };

    /// <summary>Namespaces the given layer must never depend on.</summary>
    public static IReadOnlyList<string> ForbiddenNamespaces(Layer layer, string? module, IReadOnlyCollection<string> allModules)
    {
        IEnumerable<string> forbidden = layer switch
        {
            Layer.SharedKernel => [.. Frameworks, "Hopaut.Modules", "Hopaut.BuildingBlocks"],
            Layer.IntegrationEvents => [.. Frameworks, "Hopaut.Modules", "Hopaut.BuildingBlocks"],
            Layer.BuildingBlocksApplication => [.. Frameworks, "Hopaut.Modules", "Hopaut.BuildingBlocks.Infrastructure", "Hopaut.BuildingBlocks.Api"],
            Layer.BuildingBlocksInfrastructure => ["Hopaut.Modules", "Hopaut.BuildingBlocks.Api"],
            Layer.BuildingBlocksApi => [EfCore, Hangfire, MassTransit, "Hopaut.Modules", "Hopaut.BuildingBlocks.Infrastructure"],
            Layer.Domain =>
            [
                .. Own(module, Layer.Application, Layer.Contracts, Layer.Infrastructure, Layer.Api),
                .. Others(module, allModules, SolutionAssemblies.ModuleLayers),
                .. Frameworks, DataAnnotations, "Hopaut.BuildingBlocks", "Hopaut.IntegrationEvents",
            ],
            Layer.Application =>
            [
                .. Own(module, Layer.Infrastructure, Layer.Api),
                .. Others(module, allModules, ImplementationLayers),
                .. Frameworks, "Hopaut.BuildingBlocks.Infrastructure", "Hopaut.BuildingBlocks.Api",
            ],
            Layer.Contracts =>
            [
                .. Own(module, ImplementationLayers),
                .. Others(module, allModules, ImplementationLayers),
                .. Frameworks, "Hopaut.BuildingBlocks.Application", "Hopaut.BuildingBlocks.Infrastructure", "Hopaut.BuildingBlocks.Api",
            ],
            Layer.Infrastructure =>
            [
                .. Own(module, Layer.Api),
                .. Others(module, allModules, ImplementationLayers),
                "Hopaut.BuildingBlocks.Api",
            ],
            Layer.Api =>
            [
                .. Own(module, Layer.Domain, Layer.Infrastructure),
                .. Others(module, allModules, ImplementationLayers),
                EfCore, Hangfire, MassTransit, "Hopaut.BuildingBlocks.Infrastructure",
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null),
        };

        return forbidden.Distinct(StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<Violation> Evaluate(SolutionAssembly subject) =>
        Evaluate(
            RuleFor(subject.Layer),
            subject.Assembly,
            subject.Name,
            subject.Name,
            ForbiddenNamespaces(subject.Layer, subject.Module, SolutionAssemblies.ModuleNames));

    /// <summary>Reports one violation per forbidden namespace that any type in <paramref name="rootNamespace"/> depends on.</summary>
    public static IReadOnlyList<Violation> Evaluate(
        string ruleId, Assembly assembly, string rootNamespace, string subjectName, IReadOnlyList<string> forbiddenNamespaces)
    {
        var types = Types.InAssembly(assembly);
        var combined = types.That().ResideInNamespace(rootNamespace)
            .ShouldNot().HaveDependencyOnAny([.. forbiddenNamespaces]).GetResult();
        if (combined.IsSuccessful)
        {
            return [];
        }

        var violations = new List<Violation>();
        foreach (var forbidden in forbiddenNamespaces)
        {
            var result = types.That().ResideInNamespace(rootNamespace)
                .ShouldNot().HaveDependencyOn(forbidden).GetResult();
            if (!result.IsSuccessful)
            {
                violations.Add(new Violation(
                    $"{ruleId}: {subjectName} -> {forbidden}",
                    "types: " + string.Join(", ", result.FailingTypeNames ?? [])));
            }
        }

        return violations;
    }

    private static IEnumerable<string> Own(string? module, params Layer[] layers) =>
        module is null ? [] : layers.Select(l => SolutionAssemblies.ModuleAssemblyName(module, l));

    private static IEnumerable<string> Others(string? module, IReadOnlyCollection<string> allModules, Layer[] layers) =>
        allModules.Where(m => m != module).SelectMany(m => layers.Select(l => SolutionAssemblies.ModuleAssemblyName(m, l)));
}
