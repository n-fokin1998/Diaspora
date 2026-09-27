using System.Security.Cryptography;
using System.Text;
using Diaspora.Identity.Application.Common.Abstractions;

namespace Diaspora.Identity.Infrastructure.Authentication;

internal class RefreshTokenService(JwtOptions options) : IRefreshTokenService
{
    private const int TokenSizeBytes = 32;

    public IssuedRefreshToken Issue()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenSizeBytes))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var expiresAtUtc = DateTime.UtcNow.AddDays(options.RefreshTokenDays);

        return new IssuedRefreshToken(rawToken, Hash(rawToken), expiresAtUtc);
    }

    public string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
