using Hopaut.ArchitectureTests.Rules;

namespace Hopaut.ArchitectureTests;

/// <summary>
/// Compares a rule's actual violations with <see cref="ArchitectureBaseline"/> (decision D-002 in TODO.md).
/// Fails on every violation that is not baselined and on every baselined violation that no longer occurs,
/// so the baseline can only shrink.
/// </summary>
public static class Ratchet
{
    public static void Verify(string ruleId, IEnumerable<Violation> violations)
    {
        var (unexpected, stale) = Compare(violations, ArchitectureBaseline.ForRule(ruleId));
        if (unexpected.Count == 0 && stale.Count == 0)
        {
            return;
        }

        var message = new List<string> { $"Rule {ruleId} does not match the architecture baseline." };
        if (unexpected.Count > 0)
        {
            message.Add($"New violations ({unexpected.Count}) – fix them; never add them to the baseline:");
            message.AddRange(unexpected.Select(v => "  + " + v));
        }

        if (stale.Count > 0)
        {
            message.Add($"Fixed violations still baselined ({stale.Count}) – remove them from ArchitectureBaseline.cs and update the finding in TODO.md:");
            message.AddRange(stale.Select(v => "  - " + v));
        }

        Assert.Fail(string.Join(Environment.NewLine, message));
    }

    public static (IReadOnlyList<string> Unexpected, IReadOnlyList<string> Stale) Compare(
        IEnumerable<Violation> violations, IEnumerable<BaselineEntry> baseline)
    {
        var actual = violations.ToList();
        var actualIds = actual.Select(v => v.Id).ToHashSet(StringComparer.Ordinal);
        var baselined = baseline.ToDictionary(e => e.Violation, e => e.FindingId, StringComparer.Ordinal);

        var unexpected = actual.Where(v => !baselined.ContainsKey(v.Id)).Select(v => v.ToString()).Order(StringComparer.Ordinal).ToList();
        var stale = baselined.Where(e => !actualIds.Contains(e.Key)).Select(e => $"{e.Key}  [{e.Value}]").Order(StringComparer.Ordinal).ToList();
        return (unexpected, stale);
    }
}
