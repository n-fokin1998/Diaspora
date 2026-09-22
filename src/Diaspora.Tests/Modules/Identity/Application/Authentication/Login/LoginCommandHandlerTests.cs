using Diaspora.Identity.Application.Authentication.Login;
using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.Users;
using Moq;

namespace Diaspora.Tests.Modules.Identity.Application.Authentication.Login;

public class LoginCommandHandlerTests
{
    private const string RegisteredEmail = "jane@example.com";
    private const string CorrectPassword = "Str0ngPass1";

    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        var registeredUser = User.Register(
            RegisteredEmail, [1, 2, 3], [4, 5, 6], 210_000,
            "Jane", "Doe", new DateOnly(1998, 4, 12), "Berlin",
            DateTime.UtcNow);

        _userRepository
            .Setup(r => r.FindByNormalizedEmailAsync(registeredUser.NormalizedEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registeredUser);
        _userRepository
            .Setup(r => r.FindByNormalizedEmailAsync(
                It.Is<string>(e => e != registeredUser.NormalizedEmail), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasher
            .Setup(h => h.Verify(CorrectPassword, It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<int>()))
            .Returns(true);
        _passwordHasher
            .Setup(h => h.Verify(
                It.Is<string>(p => p != CorrectPassword), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<int>()))
            .Returns(false);

        _jwtTokenService
            .Setup(s => s.IssueAccessToken(It.IsAny<User>()))
            .Returns(new IssuedToken("fake-token", DateTime.UtcNow.AddHours(1)));

        _sut = new LoginCommandHandler(_userRepository.Object, _passwordHasher.Object, _jwtTokenService.Object);
    }

    [Fact]
    public async Task Handle_WithCorrectCredentials_ReturnsAccessToken()
    {
        var result = await _sut.Handle(new LoginCommand(RegisteredEmail, CorrectPassword), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(RegisteredEmail, result.Email);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
    }

    [Fact]
    public async Task Handle_WithCorrectCredentialsButDifferentCaseAndWhitespaceEmail_Succeeds()
    {
        var result = await _sut.Handle(new LoginCommand("  JANE@EXAMPLE.COM  ", CorrectPassword), CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsGenericInvalidCredentials()
    {
        var result = await _sut.Handle(new LoginCommand(RegisteredEmail, "WrongPassword1"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Empty(result.FieldErrors);
    }

    [Fact]
    public async Task Handle_WithUnregisteredEmail_ReturnsTheSameGenericInvalidCredentials()
    {
        var result = await _sut.Handle(new LoginCommand("nobody@example.com", CorrectPassword), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Empty(result.FieldErrors);
    }
}
