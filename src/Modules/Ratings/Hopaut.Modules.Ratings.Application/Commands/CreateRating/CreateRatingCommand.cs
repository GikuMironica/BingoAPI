using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Ratings.Application.Commands.CreateRating;

public sealed record CreateRatingCommand(int Rate, UserId UserId, UserId RaterId, PostId PostId, string? Feedback) : IRequest<RatingId>;
