using Hopaut.Modules.Identity.Domain;

namespace Hopaut.Modules.Identity.Application;

/// <summary>Refresh-token store. Changes are saved by the Identity unit of work, never by the repository.</summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    void MarkUsed(RefreshToken refreshToken);

    /// <summary>Tracks the new token; its key is generated when it is added, so it is known before the save.</summary>
    void Add(RefreshToken refreshToken);
}
