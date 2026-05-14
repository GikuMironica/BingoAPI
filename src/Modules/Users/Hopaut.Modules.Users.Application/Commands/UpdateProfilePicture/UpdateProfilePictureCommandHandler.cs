using MediatR;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, bool>
{
    private readonly IUserProfileRepository _repo;

    public UpdateProfilePictureCommandHandler(IUserProfileRepository repo) => _repo = repo;

    public async Task<bool> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repo.GetByIdentityUserIdAsync(request.IdentityUserId, cancellationToken);
        if (profile is null) return false;

        profile.UpdateProfilePicture(request.PictureUrl);
        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
