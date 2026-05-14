using MediatR;

namespace Hopaut.Modules.Ratings.Application.Commands.DeleteRating;

public sealed record DeleteRatingCommand(int RatingId) : IRequest<bool>;
