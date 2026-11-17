using Hopaut.Modules.Identity.Application;
using Hopaut.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Identity.Infrastructure;

public sealed class RefreshTokenRepository(IdentityModuleDbContext dbContext) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token, ct);

    public void MarkUsed(RefreshToken refreshToken) => refreshToken.Used = true;

    public void Add(RefreshToken refreshToken) => dbContext.RefreshTokens.Add(refreshToken);
}
