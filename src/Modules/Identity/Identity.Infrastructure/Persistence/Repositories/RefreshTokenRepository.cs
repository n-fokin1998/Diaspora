using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.RefreshTokens;
using Microsoft.EntityFrameworkCore;

namespace Diaspora.Identity.Infrastructure.Persistence.Repositories
{
    internal class RefreshTokenRepository(IdentityDbContext dbContext) : IRefreshTokenRepository
    {
        public Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
            dbContext.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        public void AddRefreshToken(RefreshToken refreshToken) => dbContext.RefreshTokens.Add(refreshToken);

        public async Task RevokeAllActiveForUserAsync(Guid userId, DateTime revokedAtUtc, CancellationToken cancellationToken = default)
        {
            var activeTokens = await dbContext.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.Revoke(revokedAtUtc);
            }
        }
    }
}
