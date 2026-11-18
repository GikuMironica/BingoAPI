namespace Hopaut.SharedKernel;

/// <summary>The title of a post: trimmed, 1 to <see cref="MaxLength"/> characters (the legacy rule).</summary>
public sealed record PostTitle : ISingleValueObject<PostTitle, string>
{
    public const int MaxLength = 30;

    private PostTitle(string value) => Value = value;

    public string Value { get; }

    public static Result<PostTitle> Create(string? value)
    {
        var title = value?.Trim();
        if (string.IsNullOrEmpty(title))
            return PostTextErrors.TitleRequired;

        return title.Length <= MaxLength ? new PostTitle(title) : PostTextErrors.TitleTooLong;
    }

    public override string ToString() => Value;
}

/// <summary>The description of a post: trimmed, <see cref="MinLength"/> to <see cref="MaxLength"/> characters (the legacy rules).</summary>
public sealed record PostDescription : ISingleValueObject<PostDescription, string>
{
    public const int MinLength = 10;
    public const int MaxLength = 5000;

    private PostDescription(string value) => Value = value;

    public string Value { get; }

    public static Result<PostDescription> Create(string? value)
    {
        var description = value?.Trim() ?? string.Empty;
        if (description.Length < MinLength)
            return PostTextErrors.DescriptionTooShort;

        return description.Length <= MaxLength ? new PostDescription(description) : PostTextErrors.DescriptionTooLong;
    }

    public override string ToString() => Value;
}

public static class PostTextErrors
{
    public static readonly Error TitleRequired =
        Error.Validation("shared.post_title.required", "A title is required.");

    public static readonly Error TitleTooLong =
        Error.Validation("shared.post_title.too_long", $"A title can have at most {PostTitle.MaxLength} characters.");

    public static readonly Error DescriptionTooShort =
        Error.Validation("shared.post_description.too_short", $"A description needs at least {PostDescription.MinLength} characters.");

    public static readonly Error DescriptionTooLong =
        Error.Validation("shared.post_description.too_long", $"A description can have at most {PostDescription.MaxLength} characters.");
}
