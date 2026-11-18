using System.Diagnostics.CodeAnalysis;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

/// <summary>Guid-backed ID for exercising the shared Guid rules; module IDs such as MediaAssetId follow the same shape.</summary>
public readonly record struct SampleGuidId(Guid Value) : IStronglyTypedId<SampleGuidId, Guid>
{
    public static SampleGuidId From(Guid value) => new(value);

    public static SampleGuidId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<SampleGuidId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out SampleGuidId result) =>
        StronglyTypedId.TryParseGuid(s, out result);
}
