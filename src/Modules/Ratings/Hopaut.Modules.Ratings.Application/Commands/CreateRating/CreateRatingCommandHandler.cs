using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Ratings.Application.Commands.CreateRating;

public sealed class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, RatingId>
{
    private readonly IRatingRepository _ratingRepo;
    private readonly IUserReputationRepository _reputationRepo;

    public CreateRatingCommandHandler(IRatingRepository ratingRepo, IUserReputationRepository reputationRepo)
    {
        _ratingRepo = ratingRepo;
        _reputationRepo = reputationRepo;
    }

    public async Task<RatingId> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
    {
        var rating = Rating.Create(
            RatingValue.From(request.Rate),
            request.UserId,
            request.RaterId,
            request.PostId,
            request.Feedback);

        await _ratingRepo.AddAsync(rating, cancellationToken);
        await _ratingRepo.SaveChangesAsync(cancellationToken);

        // Update denormalized reputation projection
        await _reputationRepo.UpsertAsync(request.UserId, cancellationToken);
        await _reputationRepo.SaveChangesAsync(cancellationToken);

        return rating.Id;
    }
}
