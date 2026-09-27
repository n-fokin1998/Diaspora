namespace Diaspora.Identity.Domain.RefreshTokens;

public class RefreshToken
{
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    public static RefreshToken Issue(Guid userId, string tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException("Expiry must be after creation.", nameof(expiresAtUtc));
        }

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc,
        };
    }

    public void Revoke(DateTime revokedAtUtc, string? replacedByTokenHash = null)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
