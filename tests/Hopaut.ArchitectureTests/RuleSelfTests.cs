using Hopaut.ArchitectureTests.Catalog;
using Hopaut.ArchitectureTests.Fixtures.BadMessaging;
using Hopaut.ArchitectureTests.Fixtures.BadModel;
using Hopaut.ArchitectureTests.Fixtures.GoodDomain;
using Hopaut.ArchitectureTests.Fixtures.GoodMessaging;
using Hopaut.ArchitectureTests.Rules;

namespace Hopaut.ArchitectureTests;

/// <summary>Negative tests: every rule reports a deliberately bad fixture and accepts a clean one.</summary>
public sealed class RuleSelfTests
{
    private const string BadDomainNamespace = "Hopaut.ArchitectureTests.Fixtures.BadDomain";
    private const string GoodDomainNamespace = "Hopaut.ArchitectureTests.Fixtures.GoodDomain";

    private static readonly IReadOnlyList<string> DomainRuleForPosts =
        LayerRules.ForbiddenNamespaces(Layer.Domain, "Posts", ["Posts", "Ratings"]);

    [Theory]
    [InlineData(LayerRules.EfCore)]
    [InlineData(LayerRules.AspNetCore)]
    [InlineData(LayerRules.DataAnnotations)]
    [InlineData("Hopaut.Modules.Ratings.Domain")]
    public void Layer_rule_reports_each_forbidden_dependency(string forbidden)
    {
        var violations = LayerRules.Evaluate("SELFTEST", typeof(RuleSelfTests).Assembly, BadDomainNamespace, "fixture", DomainRuleForPosts);

        Assert.Contains(violations, v => v.Id == $"SELFTEST: fixture -> {forbidden}");
    }

    [Fact]
    public void Layer_rule_accepts_a_namespace_without_forbidden_dependencies()
    {
        var violations = LayerRules.Evaluate("SELFTEST", typeof(RuleSelfTests).Assembly, GoodDomainNamespace, "fixture", DomainRuleForPosts);

        Assert.Empty(violations);
    }

    [Fact]
    public void Layer_table_forbids_other_modules_but_not_their_contracts_for_application()
    {
        var forbidden = LayerRules.ForbiddenNamespaces(Layer.Application, "Posts", ["Posts", "Ratings"]);

        Assert.Contains("Hopaut.Modules.Ratings.Application", forbidden);
        Assert.Contains("Hopaut.Modules.Posts.Infrastructure", forbidden);
        Assert.DoesNotContain("Hopaut.Modules.Ratings.Contracts", forbidden);
        Assert.DoesNotContain("Hopaut.Modules.Posts.Domain", forbidden);
    }

    [Fact]
    public void Model_rules_report_a_leaky_aggregate()
    {
        Type[] fixture = [typeof(LeakyAggregate)];

        Assert.Contains(DomainModelRules.PublicSetters(fixture), v => v.Id.EndsWith("LeakyAggregate.Name", StringComparison.Ordinal));
        Assert.Contains(DomainModelRules.PublicMutableCollections(fixture), v => v.Id.EndsWith("LeakyAggregate.Items", StringComparison.Ordinal));
        Assert.Contains(DomainModelRules.AggregatePublicConstructors(fixture), v => v.Id.EndsWith("LeakyAggregate", StringComparison.Ordinal));
    }

    [Fact]
    public void Model_rules_accept_a_clean_aggregate()
    {
        Type[] fixture = [typeof(CleanAggregate)];

        Assert.Empty(DomainModelRules.PublicSetters(fixture));
        Assert.Empty(DomainModelRules.PublicMutableCollections(fixture));
        Assert.Empty(DomainModelRules.AggregatePublicConstructors(fixture));
        Assert.Empty(DomainModelRules.PublicEventRaisers(fixture));
    }

    [Fact]
    public void Event_raiser_rule_reports_a_public_raise_method() =>
        Assert.Contains(
            DomainModelRules.PublicEventRaisers([typeof(PublicEventRaiser)]),
            v => v.Id.EndsWith("PublicEventRaiser.Raise", StringComparison.Ordinal));

    [Fact]
    public void Namespace_rule_reports_types_outside_the_assembly_root_namespace()
    {
        var subject = new SolutionAssembly(typeof(RuleSelfTests).Assembly, "Hopaut.ArchitectureTests.Fixtures", Layer.Domain, "Fixtures");

        Assert.Contains(DomainModelRules.NamespacesMatchAssembly(subject), v => v.Id.EndsWith(typeof(RuleSelfTests).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public void Ratchet_reports_new_violations_and_stale_baseline_entries()
    {
        Violation[] actual = [new("SELFTEST: new", string.Empty), new("SELFTEST: known", string.Empty)];
        BaselineEntry[] baseline = [new("SELFTEST: known", "F-000"), new("SELFTEST: fixed", "F-000")];

        var (unexpected, stale) = Ratchet.Compare(actual, baseline);

        Assert.Equal(["SELFTEST: new"], unexpected);
        Assert.Equal(["SELFTEST: fixed  [F-000]"], stale);
    }

    [Fact]
    public void Messaging_rules_report_a_command_that_does_not_return_a_result() =>
        Assert.Contains(
            MessagingRules.NonResultResponses([typeof(ReturnsBool)]),
            v => v.Id == $"{MessagingRules.ResultResponse}: {typeof(ReturnsBool).FullName}");

    [Fact]
    public void Messaging_rules_report_commands_without_exactly_their_module_marker()
    {
        var violations = MessagingRules.CommandMarkers("Fixtures", [typeof(WithoutMarker), typeof(WithTwoMarkers)]).ToList();

        Assert.Equal(2, violations.Count);
    }

    [Fact]
    public void Messaging_rules_report_a_handler_the_Host_generator_cannot_see() =>
        Assert.Contains(
            MessagingRules.NonPublicMessagingTypes([typeof(HiddenHandler)]),
            v => v.Id.EndsWith(nameof(HiddenHandler), StringComparison.Ordinal));

    [Fact]
    public void Messaging_rules_accept_a_clean_command_and_handler()
    {
        Type[] fixture = [typeof(GoodCommand), typeof(GoodCommandHandler)];

        Assert.Empty(MessagingRules.NonResultResponses(fixture));
        Assert.Empty(MessagingRules.CommandMarkers("Fixtures", fixture));
        Assert.Empty(MessagingRules.NonPublicMessagingTypes(fixture));
    }

    [Fact]
    public void Generator_rule_reports_every_project_but_the_Host()
    {
        var directory = Directory.CreateTempSubdirectory("hopaut-arch-");
        try
        {
            const string generator = """<Project><ItemGroup><PackageReference Include="Mediator.SourceGenerator" /></ItemGroup></Project>""";
            var host = Path.Combine(directory.FullName, MessagingRules.HostProject + ".csproj");
            var module = Path.Combine(directory.FullName, "Hopaut.Modules.Posts.Application.csproj");
            File.WriteAllText(host, generator);
            File.WriteAllText(module, generator);

            var violations = MessagingRules.GeneratorReferences([host, module]).Select(v => v.Id);

            Assert.Equal([$"{MessagingRules.GeneratorInHostOnly}: Hopaut.Modules.Posts.Application"], violations);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
