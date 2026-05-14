using MediatR;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfilePicture;

public sealed record UpdateProfilePictureCommand(string IdentityUserId, string PictureUrl) : IRequest<bool>;
