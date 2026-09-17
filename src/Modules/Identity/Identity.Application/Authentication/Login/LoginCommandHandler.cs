using Identity.Application.Common.Abstractions;
using Identity.Domain.Users;
using MediatR;

namespace Identity.Application.Authentication.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, LoginResult>
{
    // Used to verify a password against when no account exists, so a response doesn't leak
    // via timing whether the email is registered (FR-011: never reveal which part was wrong).
    private static readonly byte[] DummyHash = new byte[32];
    private static readonly byte[] DummySalt = new byte[16];
    private const int DummyIterations = 600_000;

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
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
            return LoginResult.ValidationFailed(errors);
        }

        var normalizedEmail = request.Email.NormalizeEmail();
        var user = await userRepository.FindByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            passwordHasher.Verify(request.Password, DummyHash, DummySalt, DummyIterations);
            return LoginResult.InvalidCredentials();
        }

        var passwordValid = passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt, user.PasswordHashIterations);
        if (!passwordValid)
        {
            return LoginResult.InvalidCredentials();
        }

        var issuedToken = jwtTokenService.IssueAccessToken(user);
        return LoginResult.Success(user.Id, user.Email, user.FirstName, user.LastName, issuedToken.AccessToken, issuedToken.ExpiresAtUtc);
    }
}
