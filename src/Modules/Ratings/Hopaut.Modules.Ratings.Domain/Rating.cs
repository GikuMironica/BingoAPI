using Hopaut.Modules.Ratings.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain;

public sealed class Rating : AggregateRoot<RatingId>
{
    public RatingValue Rate { get; private set; }
    public UserId UserId { get; private set; }
    public UserId RaterId { get; private set; }
    public PostId PostId { get; private set; }
    public string? Feedback { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Rating() { } // EF Core

    /// <summary>Creates a rating with an ID taken from the ratings sequence, so <see cref="RatingCreatedDomainEvent"/> carries it.</summary>
    public static Rating Create(RatingId id, RatingValue rate, UserId userId, UserId raterId, PostId postId, string? feedback, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);
        ArgumentNullException.ThrowIfNull(rate);

        var rating = new Rating
        {
            Id = id,
            Rate = rate,
            UserId = userId,
            RaterId = raterId,
            PostId = postId,
            Feedback = feedback,
            CreatedAt = now
        };

        rating.Raise(new RatingCreatedDomainEvent(now, id, userId, raterId, postId));

        return rating;
    }
}
