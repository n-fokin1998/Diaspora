using Diaspora.Identity.Application.Authentication.Register;
using Diaspora.Identity.Domain.Users;
using Identity.Application.Authentication.Register;

namespace Diaspora.Tests.Modules.Identity.Application.Authentication.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ReturnsNoErrors()
    {
        var errors = _sut.Validate(ValidCommand());

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithMismatchedConfirmPassword_ReturnsFieldError()
    {
        var command = ValidCommand() with { ConfirmPassword = "Different1" };

        var errors = _sut.Validate(command);

        Assert.Contains("confirmPassword", errors.Keys);
    }

    [Theory]
    [InlineData("short1")]
    [InlineData("nodigitshere")]
    [InlineData("12345678")]
    public void Validate_WithWeakPassword_ReturnsFieldError(string weakPassword)
    {
        var command = ValidCommand() with { Password = weakPassword, ConfirmPassword = weakPassword };

        var errors = _sut.Validate(command);

        Assert.Contains("password", errors.Keys);
    }

    [Fact]
    public void Validate_WithMalformedEmail_ReturnsFieldError()
    {
        var command = ValidCommand() with { Email = "not-an-email" };

        var errors = _sut.Validate(command);

        Assert.Contains("email", errors.Keys);
    }

    [Theory]
    [InlineData("", "Doe", "firstName")]
    [InlineData("Jane", "", "lastName")]
    public void Validate_WithEmptyName_ReturnsFieldError(string firstName, string lastName, string expectedField)
    {
        var command = ValidCommand() with { FirstName = firstName, LastName = lastName };

        var errors = _sut.Validate(command);

        Assert.Contains(expectedField, errors.Keys);
    }

    [Fact]
    public void Validate_WithEmptyLocation_ReturnsFieldError()
    {
        var command = ValidCommand() with { Location = "   " };

        var errors = _sut.Validate(command);

        Assert.Contains("location", errors.Keys);
    }

    [Fact]
    public void Validate_WithFutureDateOfBirth_ReturnsFieldError()
    {
        var command = ValidCommand() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1) };

        var errors = _sut.Validate(command);

        Assert.Contains("dateOfBirth", errors.Keys);
    }

    [Fact]
    public void Validate_WithUnderMinimumAge_ReturnsFieldError()
    {
        var tooYoungDateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-User.MinAgeYears + 1);
        var command = ValidCommand() with { DateOfBirth = tooYoungDateOfBirth };

        var errors = _sut.Validate(command);

        Assert.Contains("dateOfBirth", errors.Keys);
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
