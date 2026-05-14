using Hopaut.Modules.Users.Domain;

namespace Hopaut.Modules.Users.Application;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken ct = default);
    Task AddAsync(UserProfile profile, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
