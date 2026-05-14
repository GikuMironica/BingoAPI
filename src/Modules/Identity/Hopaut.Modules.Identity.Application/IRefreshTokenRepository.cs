using Hopaut.Modules.Identity.Domain;

namespace Hopaut.Modules.Identity.Application;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task MarkUsedAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
}
