using Diaspora.Identity.Domain.Users;

namespace Diaspora.Identity.Application.Common.Abstractions;

public interface IJwtTokenService
{
    IssuedToken IssueAccessToken(User user);
}

public sealed record IssuedToken(string AccessToken, DateTime ExpiresAtUtc);
