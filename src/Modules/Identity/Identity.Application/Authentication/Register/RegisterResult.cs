using Diaspora.Identity.Application.Common.Abstractions;

namespace Diaspora.Identity.Application.Authentication.Register;

public sealed class RegisterResult : IValidationFailureResult<RegisterResult>
{
    private RegisterResult()
    {
    }

    public bool Succeeded { get; private init; }
    public bool EmailAlreadyInUse { get; private init; }
    public IReadOnlyDictionary<string, string[]> FieldErrors { get; private init; } = new Dictionary<string, string[]>();

    public Guid UserId { get; private init; }
    public string Email { get; private init; } = string.Empty;
    public string FirstName { get; private init; } = string.Empty;
    public string LastName { get; private init; } = string.Empty;
    public string AccessToken { get; private init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private init; }

    public static RegisterResult Success(
        Guid userId, string email, string firstName, string lastName, string accessToken, DateTime expiresAtUtc) => new()
        {
            Succeeded = true,
            UserId = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc,
        };

    public static RegisterResult ValidationFailed(IReadOnlyDictionary<string, string[]> fieldErrors) => new()
    {
        Succeeded = false,
        FieldErrors = fieldErrors,
    };

    public static RegisterResult EmailConflict() => new()
    {
        Succeeded = false,
        EmailAlreadyInUse = true,
    };
}
