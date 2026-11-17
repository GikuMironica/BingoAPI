using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Commands.CreateRating;

/// <summary>
/// The caller rates the host of a post they attended (decision D-031); the rated user and the rater are not taken from the
/// request. Inputs arrive as sent and the handler validates them.
/// </summary>
public sealed record CreateRatingCommand(int Rate, PostId PostId, string? Feedback) : ICommand<Result<RatingId>>, IRatingsCommand;
