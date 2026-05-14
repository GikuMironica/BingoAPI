using Hopaut.Modules.Ratings.Domain;
using MediatR;

namespace Hopaut.Modules.Ratings.Application.Commands.CreateRating;

public sealed class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, int>
{
    private readonly IRatingRepository _ratingRepo;
    private readonly IUserReputationRepository _reputationRepo;

    public CreateRatingCommandHandler(IRatingRepository ratingRepo, IUserReputationRepository reputationRepo)
    {
        _ratingRepo = ratingRepo;
        _reputationRepo = reputationRepo;
    }

    public async Task<int> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
    {
        var rating = new Rating
        {
            Rate = request.Rate,
            UserId = request.UserId,
            RaterId = request.RaterId,
            PostId = request.PostId,
            Feedback = request.Feedback
        };

        await _ratingRepo.AddAsync(rating, cancellationToken);
        await _ratingRepo.SaveChangesAsync(cancellationToken);

        // Update denormalized reputation projection
        await _reputationRepo.UpsertAsync(request.UserId, cancellationToken);
        await _reputationRepo.SaveChangesAsync(cancellationToken);

        return rating.Id;
    }
}
