using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Ratings.Application.Authorization;
using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Commands.DeleteRating;

public sealed class DeleteRatingCommandHandler(
    IRatingRepository ratings, IUserReputationRepository reputations, IResourceAuthorizer authorizer)
    : ICommandHandler<DeleteRatingCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteRatingCommand request, CancellationToken cancellationToken)
    {
        var rating = await ratings.GetByIdAsync(request.RatingId, cancellationToken);
        if (rating is null)
            return RatingErrors.NotFound(request.RatingId);

        if (!await authorizer.IsAuthorizedAsync(rating, RatingPolicies.MustOwnRating, cancellationToken))
            return RatingErrors.NotRater(request.RatingId);

        ratings.Remove(rating);

        // Denormalized reputation projection, saved in the same unit of work as the deletion.
        await reputations.RecalculateAsync(rating.UserId, cancellationToken);

        return Result.Success();
    }
}
