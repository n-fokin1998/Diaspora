using Diaspora.Identity.Application.Common.Abstractions;

namespace Diaspora.Identity.Application.Authentication.Login;

public sealed class LoginCommandValidator : IValidator<LoginCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(LoginCommand request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        return errors;
    }
}
