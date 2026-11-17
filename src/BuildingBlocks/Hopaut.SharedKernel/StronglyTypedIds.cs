using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Hopaut.SharedKernel;

/// <summary>
/// Strongly-typed identifier for User entities (string-backed, from ASP.NET Identity).
/// </summary>
public readonly record struct UserId(string Value) : IStronglyTypedId<UserId, string>
{
    public static UserId From(string value) => new(value);

    public static UserId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<UserId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out UserId result) =>
        StronglyTypedId.TryParseString(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out UserId result) => StronglyTypedId.TryParseString(s, out result);

    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed identifier for Post entities (int-backed).
/// </summary>
public readonly record struct PostId(int Value) : IStronglyTypedId<PostId, int>
{
    public static PostId From(int value) => new(value);

    public static PostId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<PostId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out PostId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out PostId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for Rating entities (int-backed).
/// </summary>
public readonly record struct RatingId(int Value) : IStronglyTypedId<RatingId, int>
{
    public static RatingId From(int value) => new(value);

    public static RatingId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<RatingId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out RatingId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out RatingId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for Participation entities (int-backed).
/// </summary>
public readonly record struct ParticipationId(int Value) : IStronglyTypedId<ParticipationId, int>
{
    public static ParticipationId From(int value) => new(value);

    public static ParticipationId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<ParticipationId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out ParticipationId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out ParticipationId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for Announcement entities (int-backed).
/// </summary>
public readonly record struct AnnouncementId(int Value) : IStronglyTypedId<AnnouncementId, int>
{
    public static AnnouncementId From(int value) => new(value);

    public static AnnouncementId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<AnnouncementId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out AnnouncementId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out AnnouncementId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for Bug report entities (int-backed).
/// </summary>
public readonly record struct BugReportId(int Value) : IStronglyTypedId<BugReportId, int>
{
    public static BugReportId From(int value) => new(value);

    public static BugReportId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<BugReportId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BugReportId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out BugReportId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for PostReport entities (int-backed).
/// </summary>
public readonly record struct PostReportId(int Value) : IStronglyTypedId<PostReportId, int>
{
    public static PostReportId From(int value) => new(value);

    public static PostReportId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<PostReportId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out PostReportId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out PostReportId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for UserReport entities (int-backed).
/// </summary>
public readonly record struct UserReportId(int Value) : IStronglyTypedId<UserReportId, int>
{
    public static UserReportId From(int value) => new(value);

    public static UserReportId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<UserReportId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out UserReportId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out UserReportId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for Picture entities (int-backed).
/// </summary>
public readonly record struct PictureId(int Value) : IStronglyTypedId<PictureId, int>
{
    public static PictureId From(int value) => new(value);

    public static PictureId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<PictureId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out PictureId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out PictureId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for Tag entities (int-backed).
/// </summary>
public readonly record struct TagId(int Value) : IStronglyTypedId<TagId, int>
{
    public static TagId From(int value) => new(value);

    public static TagId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<TagId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out TagId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out TagId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Strongly-typed identifier for BugScreenshot entities (int-backed).
/// </summary>
public readonly record struct BugScreenshotId(int Value) : IStronglyTypedId<BugScreenshotId, int>
{
    public static BugScreenshotId From(int value) => new(value);

    public static BugScreenshotId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<BugScreenshotId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BugScreenshotId result) =>
        StronglyTypedId.TryParseInt32(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out BugScreenshotId result) => StronglyTypedId.TryParseInt32(s, out result);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
