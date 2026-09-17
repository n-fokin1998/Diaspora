using Identity.Application.Authentication.Register;
using Identity.Application.Common.Abstractions;
using Identity.Domain.Users;
using Moq;

namespace Diaspora.Tests.Modules.Identity.Application.Authentication.Register;

public class RegisterCommandHandlerTests
{
    private readonly List<User> _users = [];
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _userRepository
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string normalizedEmail, CancellationToken _) => _users.Any(u => u.NormalizedEmail == normalizedEmail));

        _userRepository
            .Setup(r => r.AddUser(It.IsAny<User>()))
            .Callback<User>(user => _users.Add(user));

        _passwordHasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(new HashedPassword([1, 2, 3], [4, 5, 6], 210_000));

        _jwtTokenService
            .Setup(s => s.IssueAccessToken(It.IsAny<User>()))
            .Returns(new IssuedToken("fake-token", DateTime.UtcNow.AddHours(1)));

        _sut = new RegisterCommandHandler(
            _userRepository.Object, _unitOfWork.Object, _passwordHasher.Object, _jwtTokenService.Object);
    }

    private static RegisterCommand ValidCommand(string email = "jane@example.com") => new(
        Email: email,
        Password: "Str0ngPass1",
        ConfirmPassword: "Str0ngPass1",
        FirstName: "Jane",
        LastName: "Doe",
        DateOfBirth: new DateOnly(1998, 4, 12),
        Location: "Berlin, Germany");

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
    public async Task Handle_WithDuplicateEmailDifferentCaseAndWhitespace_ReturnsEmailConflict()
    {
        await _sut.Handle(ValidCommand("jane@example.com"), CancellationToken.None);

        var result = await _sut.Handle(ValidCommand("  JANE@EXAMPLE.COM  "), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.EmailAlreadyInUse);
        Assert.Single(_users);
    }

    [Fact]
    public async Task Handle_WithMismatchedConfirmPassword_ReturnsFieldError()
    {
        var command = ValidCommand() with { ConfirmPassword = "Different1" };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("confirmPassword", result.FieldErrors.Keys);
    }

    [Theory]
    [InlineData("short1")]
    [InlineData("nodigitshere")]
    [InlineData("12345678")]
    public async Task Handle_WithWeakPassword_ReturnsFieldError(string weakPassword)
    {
        var command = ValidCommand() with { Password = weakPassword, ConfirmPassword = weakPassword };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("password", result.FieldErrors.Keys);
    }

    [Fact]
    public async Task Handle_WithMalformedEmail_ReturnsFieldError()
    {
        var command = ValidCommand() with { Email = "not-an-email" };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("email", result.FieldErrors.Keys);
    }

    [Theory]
    [InlineData("", "Doe", "firstName")]
    [InlineData("Jane", "", "lastName")]
    public async Task Handle_WithEmptyName_ReturnsFieldError(string firstName, string lastName, string expectedField)
    {
        var command = ValidCommand() with { FirstName = firstName, LastName = lastName };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(expectedField, result.FieldErrors.Keys);
    }

    [Fact]
    public async Task Handle_WithEmptyLocation_ReturnsFieldError()
    {
        var command = ValidCommand() with { Location = "   " };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("location", result.FieldErrors.Keys);
    }

    [Fact]
    public async Task Handle_WithFutureDateOfBirth_ReturnsFieldError()
    {
        var command = ValidCommand() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1) };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("dateOfBirth", result.FieldErrors.Keys);
    }

    [Fact]
    public async Task Handle_WithUnderMinimumAge_ReturnsFieldError()
    {
        var tooYoungDateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-User.MinAgeYears + 1);
        var command = ValidCommand() with { DateOfBirth = tooYoungDateOfBirth };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("dateOfBirth", result.FieldErrors.Keys);
    }
}
