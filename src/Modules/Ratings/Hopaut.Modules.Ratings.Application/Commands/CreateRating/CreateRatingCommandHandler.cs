using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Commands.CreateRating;

public sealed class CreateRatingCommandHandler(IRatingRepository ratings, IUserReputationRepository reputations, TimeProvider timeProvider)
    : ICommandHandler<CreateRatingCommand, Result<RatingId>>
{
    public async ValueTask<Result<RatingId>> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
    {
        var id = await ratings.NextIdAsync(cancellationToken);
        var rating = Rating.Create(
            id,
            RatingValue.From(request.Rate),
            request.UserId,
            request.RaterId,
            request.PostId,
            request.Feedback,
            timeProvider.GetUtcNow());

        ratings.Add(rating);

        // Denormalized reputation projection, saved in the same unit of work as the rating.
        await reputations.RecalculateAsync(request.UserId, cancellationToken);

        return rating.Id;
    }
}
