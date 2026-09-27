using Diaspora.Identity.Application.Common.Abstractions;
using MediatR;

namespace Diaspora.Identity.Application.Authentication.Logout;

public class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IRefreshTokenService refreshTokenService) : IRequestHandler<LogoutCommand, LogoutResult>
{
    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return LogoutResult.Success();
        }

        var tokenHash = refreshTokenService.Hash(request.RefreshToken);
        var existing = await refreshTokenRepository.FindByTokenHashAsync(tokenHash, cancellationToken);

        if (existing is not null && existing.RevokedAtUtc is null)
        {
            existing.Revoke(DateTime.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return LogoutResult.Success();
    }
}
