using Hopaut.ArchitectureTests.Catalog;
using Hopaut.ArchitectureTests.Fixtures.BadModel;
using Hopaut.ArchitectureTests.Fixtures.GoodDomain;
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
}
