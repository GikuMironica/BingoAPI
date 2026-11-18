using Hopaut.Modules.Ratings.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain;

/// <summary>
/// An attendee's rating of the host of a post (rules recovered from the legacy API, decision D-031): the rater was accepted
/// to the post, is not its host and rates it once.
/// </summary>
public sealed class Rating : AggregateRoot<RatingId>
{
    private Rating() { } // EF Core

    public RatingValue Rate { get; private set; }

    /// <summary>The rated user: the host of <see cref="PostId"/>.</summary>
    public UserId UserId { get; private set; }

    public UserId RaterId { get; private set; }

    public PostId PostId { get; private set; }

    public RatingFeedback? Feedback { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Rates the host of the post with an ID taken from the ratings sequence, so <see cref="RatingCreatedDomainEvent"/> carries it.</summary>
    public static Result<Rating> Create(
        RatingId id, RatingValue rate, RatedPost post, UserId rater, RatingFeedback? feedback, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);
        ArgumentNullException.ThrowIfNull(rate);
        ArgumentNullException.ThrowIfNull(post);

        if (rater == post.HostId)
            return RatingErrors.SelfRating(post.PostId);

        if (!post.RaterAttended)
            return RatingErrors.NotAttendee(post.PostId);

        if (post.RaterAlreadyRated)
            return RatingErrors.AlreadyRated(post.PostId);

        var rating = new Rating
        {
            Id = id,
            Rate = rate,
            UserId = post.HostId,
            RaterId = rater,
            PostId = post.PostId,
            Feedback = feedback,
            CreatedAt = now,
        };

        rating.Raise(new RatingCreatedDomainEvent(now, id, post.HostId, rater, post.PostId, rate));
        return rating;
    }

    /// <summary>Marks the rating as deleted; the repository removes it in the same unit of work.</summary>
    public void Delete(DateTimeOffset now) => Raise(new RatingDeletedDomainEvent(now, Id, UserId, Rate));
}
