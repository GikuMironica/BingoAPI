using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain;

/// <summary>A rater's written feedback: trimmed, 1 to <see cref="MaxLength"/> characters (the legacy rule).</summary>
public sealed record RatingFeedback : ISingleValueObject<RatingFeedback, string>
{
    public const int MaxLength = 300;

    private RatingFeedback(string value) => Value = value;

    public string Value { get; }

    public static Result<RatingFeedback> Create(string? value)
    {
        var feedback = value?.Trim();
        if (string.IsNullOrEmpty(feedback))
            return RatingErrors.FeedbackEmpty;

        return feedback.Length <= MaxLength ? new RatingFeedback(feedback) : RatingErrors.FeedbackTooLong;
    }

    public override string ToString() => Value;
}

/// <summary>
/// What the rating rules need to know about the rated post from the rater's side, gathered by the handler: who hosts it,
/// whether the rater's attendance was accepted and whether the rater already rated it.
/// </summary>
public sealed record RatedPost(PostId PostId, UserId HostId, bool RaterAttended, bool RaterAlreadyRated);
