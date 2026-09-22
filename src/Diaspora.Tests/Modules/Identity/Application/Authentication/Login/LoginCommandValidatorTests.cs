using Diaspora.Identity.Application.Authentication.Login;

namespace Diaspora.Tests.Modules.Identity.Application.Authentication.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ReturnsNoErrors()
    {
        var errors = _sut.Validate(new LoginCommand("jane@example.com", "Str0ngPass1"));

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("", "somepassword")]
    [InlineData("   ", "somepassword")]
    [InlineData("jane@example.com", "")]
    [InlineData("jane@example.com", "   ")]
    public void Validate_WithMissingField_ReturnsFieldError(string email, string password)
    {
        var errors = _sut.Validate(new LoginCommand(email, password));

        Assert.NotEmpty(errors);
    }
}
