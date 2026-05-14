using Hopaut.Modules.Identity.Application;
using Hopaut.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Identity.Infrastructure;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityModuleDbContext _dbContext;

    public RefreshTokenRepository(IdentityModuleDbContext dbContext) => _dbContext = dbContext;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        await _dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token, ct);

    public async Task MarkUsedAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        refreshToken.Used = true;
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}
