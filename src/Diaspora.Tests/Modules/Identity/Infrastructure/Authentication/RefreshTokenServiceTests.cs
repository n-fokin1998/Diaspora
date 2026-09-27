using Diaspora.Identity.Infrastructure.Authentication;

namespace Diaspora.Tests.Modules.Identity.Infrastructure.Authentication;

public class RefreshTokenServiceTests
{
    private readonly JwtOptions _options = new()
    {
        Key = "super-secret-test-signing-key-used-only-in-tests-1234567890",
        Issuer = "Diaspora.Tests",
        Audience = "Diaspora.Tests.Audience",
        AccessTokenMinutes = 60,
        RefreshTokenDays = 14,
    };

    [Fact]
    public void Issue_CalledTwice_ProducesDifferentRawTokensAndHashes()
    {
        var sut = new RefreshTokenService(_options);

        var first = sut.Issue();
        var second = sut.Issue();

        Assert.NotEqual(first.RawToken, second.RawToken);
        Assert.NotEqual(first.TokenHash, second.TokenHash);
    }

    [Fact]
    public void Issue_ReturnsExpiryMatchingConfiguredRefreshTokenDays()
    {
        var sut = new RefreshTokenService(_options);

        var issued = sut.Issue();

        var expectedExpiry = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);
        Assert.Equal(expectedExpiry, issued.ExpiresAtUtc, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Hash_CalledTwiceForSameRawToken_IsDeterministic()
    {
        var sut = new RefreshTokenService(_options);

        var first = sut.Hash("some-raw-token");
        var second = sut.Hash("some-raw-token");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Issue_TokenHashMatchesHashOfRawToken()
    {
        var sut = new RefreshTokenService(_options);

        var issued = sut.Issue();

        Assert.Equal(sut.Hash(issued.RawToken), issued.TokenHash);
    }
}
