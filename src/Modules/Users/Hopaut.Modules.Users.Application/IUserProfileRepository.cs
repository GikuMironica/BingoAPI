using Hopaut.Modules.Users.Domain;

namespace Hopaut.Modules.Users.Application;

/// <summary>UserProfile aggregate store. Changes are saved by the Users unit of work, never by the repository.</summary>
public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken ct = default);
    void Add(UserProfile profile);
}
