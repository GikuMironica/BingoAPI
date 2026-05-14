using Hopaut.Modules.Users.Application;
using Hopaut.Modules.Users.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Users.Infrastructure;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly UsersModuleDbContext _dbContext;

    public UserProfileRepository(UsersModuleDbContext dbContext) => _dbContext = dbContext;

    public async Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken ct = default) =>
        await _dbContext.UserProfiles.SingleOrDefaultAsync(p => p.IdentityUserId == identityUserId, ct);

    public async Task AddAsync(UserProfile profile, CancellationToken ct = default) =>
        await _dbContext.UserProfiles.AddAsync(profile, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _dbContext.SaveChangesAsync(ct);
}
