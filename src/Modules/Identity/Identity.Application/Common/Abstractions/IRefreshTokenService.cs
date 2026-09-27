namespace Diaspora.Identity.Application.Common.Abstractions;

public interface IRefreshTokenService
{
    IssuedRefreshToken Issue();

    string Hash(string rawToken);
}

public sealed record IssuedRefreshToken(string RawToken, string TokenHash, DateTime ExpiresAtUtc);
