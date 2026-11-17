using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Commands.DeleteRating;

public sealed record DeleteRatingCommand(RatingId RatingId) : ICommand<Result>, IRatingsCommand;
