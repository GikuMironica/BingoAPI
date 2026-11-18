using Hopaut.Modules.Users.Application;
using Hopaut.Modules.Users.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Users.Infrastructure;

public sealed class UserProfileRepository(UsersModuleDbContext dbContext) : IUserProfileRepository
{
    public Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken ct = default) =>
        dbContext.UserProfiles.SingleOrDefaultAsync(p => p.IdentityUserId == identityUserId, ct);

    public void Add(UserProfile profile) => dbContext.UserProfiles.Add(profile);
}
