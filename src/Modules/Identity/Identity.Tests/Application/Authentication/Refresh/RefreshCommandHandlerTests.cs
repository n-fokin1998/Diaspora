using Diaspora.Identity.Application.Authentication.Refresh;
using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.RefreshTokens;
using Diaspora.Identity.Domain.Users;
using Moq;

namespace Diaspora.Identity.Tests.Application.Authentication.Refresh;

public class RefreshCommandHandlerTests
{
    private static readonly User RegisteredUser = User.Register(
        "jane@example.com", [1, 2, 3], [4, 5, 6], 210_000,
        "Jane", "Doe", new DateOnly(1998, 4, 12), "Berlin",
        DateTime.UtcNow);

    private readonly Dictionary<string, RefreshToken> _tokensByHash = [];
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenService = new();
    private readonly RefreshCommandHandler _sut;

    public RefreshCommandHandlerTests()
    {
        _userRepository
            .Setup(r => r.FindByIdAsync(RegisteredUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RegisteredUser);

        _refreshTokenRepository
            .Setup(r => r.FindByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string hash, CancellationToken _) => _tokensByHash.GetValueOrDefault(hash));

        _refreshTokenRepository
            .Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => _tokensByHash[token.TokenHash] = token);

        _refreshTokenRepository
            .Setup(r => r.RevokeAllActiveForUserAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns((Guid userId, DateTime revokedAtUtc, CancellationToken _) =>
            {
                foreach (var token in _tokensByHash.Values.Where(t => t.UserId == userId && t.RevokedAtUtc is null))
                {
                    token.Revoke(revokedAtUtc);
                }
                return Task.CompletedTask;
            });

        _jwtTokenService
            .Setup(s => s.IssueAccessToken(It.IsAny<User>()))
            .Returns(new IssuedToken("fake-access-token", DateTime.UtcNow.AddHours(1)));

        var issueCallCount = 0;
        _refreshTokenService
            .Setup(s => s.Issue())
            .Returns(() =>
            {
                issueCallCount++;
                return new IssuedRefreshToken($"new-raw-token-{issueCallCount}", $"new-token-hash-{issueCallCount}", DateTime.UtcNow.AddDays(14));
            });
        _refreshTokenService
            .Setup(s => s.Hash(It.IsAny<string>()))
            .Returns((string rawToken) => $"hash-of-{rawToken}");

        _sut = new RefreshCommandHandler(
            _userRepository.Object, _refreshTokenRepository.Object, _unitOfWork.Object,
            _jwtTokenService.Object, _refreshTokenService.Object);
    }

    private RefreshToken SeedActiveToken(
        string rawToken, DateTime? createdAtUtc = null, DateTime? expiresAtUtc = null, DateTime? revokedAtUtc = null)
    {
        var hash = $"hash-of-{rawToken}";
        var token = RefreshToken.Issue(
            RegisteredUser.Id, hash, createdAtUtc ?? DateTime.UtcNow, expiresAtUtc ?? DateTime.UtcNow.AddDays(14));
        if (revokedAtUtc is not null)
        {
            token.Revoke(revokedAtUtc.Value);
        }
        _tokensByHash[hash] = token;
        return token;
    }

    [Fact]
    public async Task Handle_WithActiveToken_RotatesAndReturnsNewTokens()
    {
        SeedActiveToken("current-raw-token");

        var result = await _sut.Handle(new RefreshCommand("current-raw-token"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        Assert.NotEqual("current-raw-token", result.RefreshToken);

        var oldToken = _tokensByHash["hash-of-current-raw-token"];
        Assert.NotNull(oldToken.RevokedAtUtc);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsInvalid()
    {
        var result = await _sut.Handle(new RefreshCommand("never-issued"), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ReturnsInvalid()
    {
        SeedActiveToken("expired-token", createdAtUtc: DateTime.UtcNow.AddDays(-15), expiresAtUtc: DateTime.UtcNow.AddSeconds(-1));

        var result = await _sut.Handle(new RefreshCommand("expired-token"), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_ReturnsInvalidAndRevokesEveryOtherActiveTokenForThatUser()
    {
        SeedActiveToken("reused-token", revokedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        var otherActiveToken = SeedActiveToken("another-still-active-token");

        var result = await _sut.Handle(new RefreshCommand("reused-token"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.NotNull(otherActiveToken.RevokedAtUtc);
    }
}
