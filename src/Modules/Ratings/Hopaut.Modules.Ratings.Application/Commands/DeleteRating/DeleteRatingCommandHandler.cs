using MediatR;

namespace Hopaut.Modules.Ratings.Application.Commands.DeleteRating;

public sealed class DeleteRatingCommandHandler : IRequestHandler<DeleteRatingCommand, bool>
{
    private readonly IRatingRepository _ratingRepo;
    private readonly IUserReputationRepository _reputationRepo;

    public DeleteRatingCommandHandler(IRatingRepository ratingRepo, IUserReputationRepository reputationRepo)
    {
        _ratingRepo = ratingRepo;
        _reputationRepo = reputationRepo;
    }

    public async Task<bool> Handle(DeleteRatingCommand request, CancellationToken cancellationToken)
    {
        var rating = await _ratingRepo.GetByIdAsync(request.RatingId, cancellationToken);
        if (rating is null) return false;

        var userId = rating.UserId;
        await _ratingRepo.DeleteAsync(rating, cancellationToken);
        await _ratingRepo.SaveChangesAsync(cancellationToken);

        await _reputationRepo.UpsertAsync(userId, cancellationToken);
        await _reputationRepo.SaveChangesAsync(cancellationToken);

        return true;
    }
}
