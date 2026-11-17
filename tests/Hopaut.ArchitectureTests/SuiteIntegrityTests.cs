using System.Text.RegularExpressions;
using Hopaut.ArchitectureTests.Catalog;

namespace Hopaut.ArchitectureTests;

/// <summary>Guards that keep the suite meaningful: nothing escapes the rules and the baseline stays honest.</summary>
public sealed partial class SuiteIntegrityTests
{
    private static readonly string[] KnownRules =
    [
        Rules.LayerRules.SharedKernel, Rules.LayerRules.IntegrationEvents, Rules.LayerRules.BuildingBlocksApplication,
        Rules.LayerRules.BuildingBlocksInfrastructure, Rules.LayerRules.BuildingBlocksApi, Rules.LayerRules.Domain,
        Rules.LayerRules.Application, Rules.LayerRules.Contracts, Rules.LayerRules.Infrastructure, Rules.LayerRules.Api,
        Rules.DomainModelRules.PublicSetter, Rules.DomainModelRules.MutableCollection, Rules.DomainModelRules.AggregateConstructor,
        Rules.DomainModelRules.EventRaiser, Rules.DomainModelRules.NamespaceMatchesAssembly,
        Rules.MessagingRules.NoMediatR, Rules.MessagingRules.ResultResponse, Rules.MessagingRules.ModuleMarker,
        Rules.MessagingRules.PublicTypes, Rules.MessagingRules.GeneratorInHostOnly,
    ];

    [Fact]
    public void Every_src_project_is_under_architecture_test()
    {
        var root = RepositoryRoot();
        var expected = new[] { Path.Combine(root, "src", "Modules"), Path.Combine(root, "src", "BuildingBlocks") }
            .SelectMany(dir => Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories))
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();
        var loaded = SolutionAssemblies.All.Select(a => a.Name).ToHashSet(StringComparer.Ordinal);

        var missing = expected.Where(name => !loaded.Contains(name!)).ToList();

        Assert.True(
            missing.Count == 0,
            "Projects not under architecture test (unknown layer name or not referenced): " + string.Join(", ", missing));
    }

    [Fact]
    public void Every_baseline_entry_belongs_to_a_known_rule_and_is_unique()
    {
        var unknown = ArchitectureBaseline.Entries.Where(e => !KnownRules.Any(r => e.Violation.StartsWith(r + ":", StringComparison.Ordinal))).ToList();
        var duplicates = ArchitectureBaseline.Entries.GroupBy(e => e.Violation).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        Assert.True(unknown.Count == 0, "Baseline entries with an unknown rule: " + string.Join(", ", unknown));
        Assert.True(duplicates.Count == 0, "Duplicate baseline entries: " + string.Join(", ", duplicates));
    }

    [Fact]
    public void Every_baseline_entry_names_an_open_finding_in_TODO()
    {
        var findings = OpenFindings(Path.Combine(RepositoryRoot(), "TODO.md"));

        var orphans = ArchitectureBaseline.Entries
            .Where(e => !findings.Contains(e.FindingId))
            .Select(e => $"{e.Violation} [{e.FindingId}]")
            .ToList();

        Assert.True(orphans.Count == 0, "Baseline entries whose finding is missing or closed in TODO.md: " + string.Join(Environment.NewLine, orphans));
    }

    internal static string RepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Bingo.sln")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException("Bingo.sln not found above " + AppContext.BaseDirectory);
    }

    /// <summary>Finding ids from the TODO.md findings table whose status column is "open" or "partly fixed".</summary>
    private static HashSet<string> OpenFindings(string todoPath) =>
        File.ReadLines(todoPath)
            .Select(line => FindingRow().Match(line))
            .Where(m => m.Success)
            .Where(m => m.Groups["status"].Value.TrimStart().StartsWith("open", StringComparison.OrdinalIgnoreCase)
                        || m.Groups["status"].Value.TrimStart().StartsWith("partly fixed", StringComparison.OrdinalIgnoreCase))
            .Select(m => m.Groups["id"].Value)
            .ToHashSet(StringComparer.Ordinal);

    [GeneratedRegex(@"^\|\s*(?<id>F-\d{3})\s*\|.*\|(?<status>[^|]*)\|\s*$")]
    private static partial Regex FindingRow();
}
