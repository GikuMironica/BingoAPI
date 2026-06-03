using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Ratings.Application.Commands.DeleteRating;

public sealed record DeleteRatingCommand(RatingId RatingId) : IRequest<bool>;
