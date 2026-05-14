using MediatR;

namespace Hopaut.Modules.Ratings.Application.Commands.CreateRating;

public sealed record CreateRatingCommand(int Rate, string UserId, string RaterId, int PostId, string? Feedback) : IRequest<int>;
