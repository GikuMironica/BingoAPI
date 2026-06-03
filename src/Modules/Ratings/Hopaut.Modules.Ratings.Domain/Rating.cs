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

    public static Rating Create(RatingValue rate, UserId userId, UserId raterId, PostId postId, string? feedback)
    {
        var rating = new Rating
        {
            Rate = rate,
            UserId = userId,
            RaterId = raterId,
            PostId = postId,
            Feedback = feedback,
            CreatedAt = DateTimeOffset.UtcNow
        };

        rating.AddDomainEvent(new RatingCreatedDomainEvent(rating.Id, userId, raterId, postId));

        return rating;
    }
}
