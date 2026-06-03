using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Posts.Application.Commands.CreatePost;

public sealed record CreatePostCommand(
    DateTimeOffset EventTime,
    DateTimeOffset? EndTime,
    UserId UserId,
    CreatePostLocationInput Location,
    CreatePostEventInput Event,
    List<string>? PictureUrls = null,
    List<string>? TagNames = null) : IRequest<PostId>;

public sealed record CreatePostLocationInput(string? EntityName, string? City, string? Region, string? Address, string? Country, double Longitude, double Latitude);

public sealed record CreatePostEventInput(EventType EventType, double? EntrancePrice, int? Currency, string? Title, string Description, string? Requirements, int? Slots, string? TypeSpecificData);
