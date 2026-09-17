using Identity.Domain.Users;

namespace Identity.Application.Common.Abstractions;

public interface IJwtTokenService
{
    IssuedToken IssueAccessToken(User user);
}

public sealed record IssuedToken(string AccessToken, DateTime ExpiresAtUtc);
