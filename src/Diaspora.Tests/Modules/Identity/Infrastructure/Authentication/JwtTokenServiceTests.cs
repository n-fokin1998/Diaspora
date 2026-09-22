using Diaspora.Identity.Domain.Users;
using Diaspora.Identity.Infrastructure.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace Diaspora.Tests.Modules.Identity.Infrastructure.Authentication;

public class JwtTokenServiceTests
{
    private readonly JwtOptions Options = new()
    {
        Key = "super-secret-test-signing-key-used-only-in-tests-1234567890",
        Issuer = "Diaspora.Tests",
        Audience = "Diaspora.Tests.Audience",
        AccessTokenMinutes = 60,
    };

    [Fact]
    public void IssueAccessToken_ReturnsTokenWithExpectedClaimsAndExpiry()
    {
        var sut = new JwtTokenService(Options);
        var user = User.Register(
            "jane@example.com", [1, 2, 3], [4, 5, 6], 210_000,
            "Jane", "Doe", new DateOnly(1998, 4, 12), "Berlin",
            DateTime.UtcNow);

        var issued = sut.IssueAccessToken(user);

        Assert.False(string.IsNullOrWhiteSpace(issued.AccessToken));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(issued.AccessToken);
        Assert.Equal(Options.Issuer, token.Issuer);
        Assert.Contains(Options.Audience, token.Audiences);
        Assert.Equal(user.Id.ToString(), token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.True(token.ValidTo > DateTime.UtcNow);
        Assert.Equal(issued.ExpiresAtUtc, token.ValidTo, TimeSpan.FromSeconds(1));
    }
}
