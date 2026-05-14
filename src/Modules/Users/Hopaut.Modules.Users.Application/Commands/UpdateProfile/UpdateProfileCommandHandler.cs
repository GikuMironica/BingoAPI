using MediatR;

namespace Hopaut.Modules.Users.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IUserProfileRepository _repo;

    public UpdateProfileCommandHandler(IUserProfileRepository repo) => _repo = repo;

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repo.GetByIdentityUserIdAsync(request.IdentityUserId, cancellationToken);
        if (profile is null) return false;

        profile.UpdateProfile(request.FirstName, request.LastName, request.Description);
        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
