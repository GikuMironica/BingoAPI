using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Commands.CreateRating;

public sealed class CreateRatingCommandHandler(
    IRatingRepository ratings,
    IPostHostQueries posts,
    IAttendanceQueries attendance,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<CreateRatingCommand, Result<RatingId>>
{
    public async ValueTask<Result<RatingId>> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
    {
        var rate = RatingValue.Create(request.Rate);
        var feedback = string.IsNullOrWhiteSpace(request.Feedback) ? null : RatingFeedback.Create(request.Feedback);

        var invalid = ValidationError.FromFailures(("rate", rate), ("feedback", feedback));
        if (invalid is not null)
            return invalid;

        var host = await posts.GetAsync(request.PostId, cancellationToken);
        if (host is null)
            return RatingErrors.PostNotFound(request.PostId);

        var rater = currentUser.Id;
        var post = new RatedPost(
            host.PostId,
            host.HostId,
            RaterAttended: await attendance.IsAcceptedAsync(host.PostId, rater, cancellationToken),
            RaterAlreadyRated: await ratings.ExistsAsync(rater, host.PostId, cancellationToken));

        var id = await ratings.NextIdAsync(cancellationToken);
        var rating = Rating.Create(id, rate.Value, post, rater, feedback?.Value, timeProvider.GetUtcNow());
        if (rating.IsFailure)
            return rating.Error;

        ratings.Add(rating.Value);
        return rating.Value.Id;
    }
}
