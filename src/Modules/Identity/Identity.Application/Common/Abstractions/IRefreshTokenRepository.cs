using Diaspora.Identity.Domain.RefreshTokens;

namespace Diaspora.Identity.Application.Common.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    void AddRefreshToken(RefreshToken refreshToken);

    Task RevokeAllActiveForUserAsync(Guid userId, DateTime revokedAtUtc, CancellationToken cancellationToken = default);
}
