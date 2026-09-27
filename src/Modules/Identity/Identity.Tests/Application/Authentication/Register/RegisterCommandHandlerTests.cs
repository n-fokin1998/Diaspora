using Diaspora.Identity.Application.Authentication.Register;
using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.RefreshTokens;
using Diaspora.Identity.Domain.Users;
using Moq;

namespace Diaspora.Identity.Tests.Application.Authentication.Register;

public class RegisterCommandHandlerTests
{
    private readonly List<User> _users = [];
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenService = new();
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _userRepository
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string normalizedEmail, CancellationToken _) => _users.Any(u => u.NormalizedEmail == normalizedEmail));

        _userRepository
            .Setup(r => r.AddUser(It.IsAny<User>()))
            .Callback<User>(user => _users.Add(user));

        _refreshTokenRepository
            .Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => _refreshTokens.Add(token));

        _passwordHasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(new HashedPassword([1, 2, 3], [4, 5, 6], 210_000));

        _jwtTokenService
            .Setup(s => s.IssueAccessToken(It.IsAny<User>()))
            .Returns(new IssuedToken("fake-token", DateTime.UtcNow.AddHours(1)));

        _refreshTokenService
            .Setup(s => s.Issue())
            .Returns(new IssuedRefreshToken("fake-refresh-token", "fake-refresh-token-hash", DateTime.UtcNow.AddDays(14)));

        _sut = new RegisterCommandHandler(
            _userRepository.Object, _refreshTokenRepository.Object, _unitOfWork.Object,
            _passwordHasher.Object, _jwtTokenService.Object, _refreshTokenService.Object);
    }

    [Fact]
    public async Task Handle_WithValidInput_CreatesUserAndReturnsAccessToken()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("jane@example.com", result.Email);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.Single(_users);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidInput_AlsoIssuesAndPersistsARefreshToken()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        Assert.Single(_refreshTokens);
        Assert.Equal(result.UserId, _refreshTokens[0].UserId);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmailDifferentCaseAndWhitespace_ReturnsEmailConflict()
    {
        await _sut.Handle(ValidCommand("jane@example.com"), CancellationToken.None);

        var result = await _sut.Handle(ValidCommand("  JANE@EXAMPLE.COM  "), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.EmailAlreadyInUse);
        Assert.Single(_users);
    }

    private RegisterCommand ValidCommand(string email = "jane@example.com") => new(
        Email: email,
        Password: "Str0ngPass1",
        ConfirmPassword: "Str0ngPass1",
        FirstName: "Jane",
        LastName: "Doe",
        DateOfBirth: new DateOnly(1998, 4, 12),
        Location: "Berlin, Germany");
}
