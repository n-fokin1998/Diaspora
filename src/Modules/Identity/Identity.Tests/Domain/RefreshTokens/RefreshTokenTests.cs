using Diaspora.Identity.Domain.RefreshTokens;

namespace Diaspora.Identity.Tests.Domain.RefreshTokens;

public class RefreshTokenTests
{
    private static readonly DateTime Now = new(2026, 9, 22, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Issue_WithValidInput_ReturnsActiveToken()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "some-hash", Now, Now.AddDays(14));

        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal("some-hash", token.TokenHash);
        Assert.Null(token.RevokedAtUtc);
        Assert.Null(token.ReplacedByTokenHash);
    }

    [Fact]
    public void Issue_WithExpiryNotAfterCreation_Throws()
    {
        Assert.Throws<ArgumentException>(() => RefreshToken.Issue(Guid.NewGuid(), "some-hash", Now, Now));
    }

    [Fact]
    public void Revoke_WithoutReplacement_MarksInactive()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "some-hash", Now, Now.AddDays(14));

        token.Revoke(Now.AddMinutes(1));

        Assert.NotNull(token.RevokedAtUtc);
        Assert.Null(token.ReplacedByTokenHash);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_WithReplacement_SetsReplacedByTokenHash()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "some-hash", Now, Now.AddDays(14));

        token.Revoke(Now.AddMinutes(1), "new-hash");

        Assert.Equal("new-hash", token.ReplacedByTokenHash);
    }

    [Fact]
    public void Revoke_CalledTwice_IsIdempotentAndKeepsFirstRevocation()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "some-hash", Now, Now.AddDays(14));

        token.Revoke(Now.AddMinutes(1), "first-hash");
        token.Revoke(Now.AddMinutes(2), "second-hash");

        Assert.Equal(Now.AddMinutes(1), token.RevokedAtUtc);
        Assert.Equal("first-hash", token.ReplacedByTokenHash);
    }

    [Fact]
    public void IsActive_OnceExpired_IsFalse()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "some-hash", Now.AddDays(-30), Now.AddDays(-29));

        Assert.False(token.IsActive);
    }
}
