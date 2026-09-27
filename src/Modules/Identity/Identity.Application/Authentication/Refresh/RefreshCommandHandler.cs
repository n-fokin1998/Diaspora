using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.RefreshTokens;
using MediatR;

namespace Diaspora.Identity.Application.Authentication.Refresh;

public class RefreshCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService) : IRequestHandler<RefreshCommand, RefreshResult>
{
    public async Task<RefreshResult> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenService.Hash(request.RefreshToken);
        var existing = await refreshTokenRepository.FindByTokenHashAsync(tokenHash, cancellationToken);

        if (existing is null)
        {
            return RefreshResult.Invalid();
        }

        if (existing.RevokedAtUtc is not null)
        {
            // The presented token was already rotated away, so this is a replay of a
            // superseded credential — the signature of a stolen token (spec.md FR-015).
            // Treat it as a possible compromise and end every active session for this user.
            await refreshTokenRepository.RevokeAllActiveForUserAsync(existing.UserId, DateTime.UtcNow, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return RefreshResult.Invalid();
        }

        if (existing.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return RefreshResult.Invalid();
        }

        var user = await userRepository.FindByIdAsync(existing.UserId, cancellationToken);
        if (user is null)
        {
            return RefreshResult.Invalid();
        }

        var issuedRefreshToken = refreshTokenService.Issue();
        var utcNow = DateTime.UtcNow;
        refreshTokenRepository.AddRefreshToken(
            RefreshToken.Issue(user.Id, issuedRefreshToken.TokenHash, utcNow, issuedRefreshToken.ExpiresAtUtc));
        existing.Revoke(utcNow, issuedRefreshToken.TokenHash);

        var issuedAccessToken = jwtTokenService.IssueAccessToken(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RefreshResult.Success(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            issuedAccessToken.AccessToken,
            issuedAccessToken.ExpiresAtUtc,
            issuedRefreshToken.RawToken,
            issuedRefreshToken.ExpiresAtUtc);
    }
}
