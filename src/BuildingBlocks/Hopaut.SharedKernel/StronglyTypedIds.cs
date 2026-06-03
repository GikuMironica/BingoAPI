namespace Hopaut.SharedKernel;

/// <summary>
/// Strongly-typed identifier for User entities (string-backed, from ASP.NET Identity).
/// </summary>
public readonly record struct UserId(string Value)
{
    public static UserId From(string value) => new(value);
    public static bool TryParse(string? input, out UserId result)
    {
        if (string.IsNullOrWhiteSpace(input)) { result = default; return false; }
        result = new UserId(input);
        return true;
    }
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed identifier for Post entities (int-backed).
/// </summary>
public readonly record struct PostId(int Value)
{
    public static PostId From(int value) => new(value);
    public static bool TryParse(string? input, out PostId result)
    {
        if (int.TryParse(input, out var v)) { result = new PostId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for Rating entities (int-backed).
/// </summary>
public readonly record struct RatingId(int Value)
{
    public static RatingId From(int value) => new(value);
    public static bool TryParse(string? input, out RatingId result)
    {
        if (int.TryParse(input, out var v)) { result = new RatingId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for Participation entities (int-backed).
/// </summary>
public readonly record struct ParticipationId(int Value)
{
    public static ParticipationId From(int value) => new(value);
    public static bool TryParse(string? input, out ParticipationId result)
    {
        if (int.TryParse(input, out var v)) { result = new ParticipationId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for Announcement entities (int-backed).
/// </summary>
public readonly record struct AnnouncementId(int Value)
{
    public static AnnouncementId From(int value) => new(value);
    public static bool TryParse(string? input, out AnnouncementId result)
    {
        if (int.TryParse(input, out var v)) { result = new AnnouncementId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for Bug report entities (int-backed).
/// </summary>
public readonly record struct BugReportId(int Value)
{
    public static BugReportId From(int value) => new(value);
    public static bool TryParse(string? input, out BugReportId result)
    {
        if (int.TryParse(input, out var v)) { result = new BugReportId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for PostReport entities (int-backed).
/// </summary>
public readonly record struct PostReportId(int Value)
{
    public static PostReportId From(int value) => new(value);
    public static bool TryParse(string? input, out PostReportId result)
    {
        if (int.TryParse(input, out var v)) { result = new PostReportId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for UserReport entities (int-backed).
/// </summary>
public readonly record struct UserReportId(int Value)
{
    public static UserReportId From(int value) => new(value);
    public static bool TryParse(string? input, out UserReportId result)
    {
        if (int.TryParse(input, out var v)) { result = new UserReportId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for Picture entities (int-backed).
/// </summary>
public readonly record struct PictureId(int Value)
{
    public static PictureId From(int value) => new(value);
    public static bool TryParse(string? input, out PictureId result)
    {
        if (int.TryParse(input, out var v)) { result = new PictureId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for Tag entities (int-backed).
/// </summary>
public readonly record struct TagId(int Value)
{
    public static TagId From(int value) => new(value);
    public static bool TryParse(string? input, out TagId result)
    {
        if (int.TryParse(input, out var v)) { result = new TagId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly-typed identifier for BugScreenshot entities (int-backed).
/// </summary>
public readonly record struct BugScreenshotId(int Value)
{
    public static BugScreenshotId From(int value) => new(value);
    public static bool TryParse(string? input, out BugScreenshotId result)
    {
        if (int.TryParse(input, out var v)) { result = new BugScreenshotId(v); return true; }
        result = default; return false;
    }
    public override string ToString() => Value.ToString();
}
