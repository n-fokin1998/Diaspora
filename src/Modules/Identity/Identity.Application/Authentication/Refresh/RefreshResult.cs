namespace Diaspora.Identity.Application.Authentication.Refresh;

public sealed class RefreshResult
{
    private RefreshResult()
    {
    }

    public bool Succeeded { get; private init; }

    public Guid UserId { get; private init; }
    public string Email { get; private init; } = string.Empty;
    public string FirstName { get; private init; } = string.Empty;
    public string LastName { get; private init; } = string.Empty;
    public string AccessToken { get; private init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private init; }
    public string RefreshToken { get; private init; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; private init; }

    public static RefreshResult Success(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        string accessToken,
        DateTime expiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc) => new()
        {
            Succeeded = true,
            UserId = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
        };

    public static RefreshResult Invalid() => new()
    {
        Succeeded = false,
    };
}
