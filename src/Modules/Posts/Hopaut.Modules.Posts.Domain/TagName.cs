using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>
/// The name of a tag: trimmed, lower-case (invariant culture, as the legacy API stored them) and 1 to
/// <see cref="MaxLength"/> characters.
/// </summary>
public sealed record TagName : ISingleValueObject<TagName, string>
{
    public const int MaxLength = 20;

    private TagName(string value) => Value = value;

    public string Value { get; }

    public static Result<TagName> Create(string? value)
    {
        var name = value?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(name))
            return TagErrors.NameRequired;

        return name.Length <= MaxLength ? new TagName(name) : TagErrors.NameTooLong;
    }

    /// <summary>
    /// The distinct tag names of one post: each name is validated and normalised, duplicates collapse, and at most
    /// <see cref="Post.MaxTags"/> remain. An absent list means no tags.
    /// </summary>
    public static Result<IReadOnlyList<TagName>> CreateMany(IEnumerable<string?>? values)
    {
        var names = new List<TagName>();
        foreach (var value in values ?? [])
        {
            var name = Create(value);
            if (name.IsFailure)
                return name.Error;

            if (!names.Contains(name.Value))
                names.Add(name.Value);
        }

        return names.Count <= Post.MaxTags ? names : PostErrors.TooManyTags;
    }

    public override string ToString() => Value;
}

public static class TagErrors
{
    public static readonly Error NameRequired =
        Error.Validation("posts.tag.name_required", "A tag needs a name.");

    public static readonly Error NameTooLong =
        Error.Validation("posts.tag.name_too_long", $"A tag can have at most {TagName.MaxLength} characters.");
}
