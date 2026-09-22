using Diaspora.Identity.Application.Authentication.Register;
using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Domain.Users;
using System.Text.RegularExpressions;

namespace Identity.Application.Authentication.Register;

public sealed partial class RegisterCommandValidator : IValidator<RegisterCommand>
{
    private const int MinPasswordLength = 8;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public IReadOnlyDictionary<string, string[]> Validate(RegisterCommand request)
    {
        var errors = new Dictionary<string, List<string>>();

        void AddError(string field, string message)
        {
            if (!errors.TryGetValue(field, out var messages))
            {
                messages = [];
                errors[field] = messages;
            }
            messages.Add(message);
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex().IsMatch(request.Email.Trim()))
        {
            AddError("email", "Enter a valid email address.");
        }

        if (string.IsNullOrEmpty(request.Password)
            || request.Password.Length < MinPasswordLength
            || !request.Password.Any(char.IsLetter)
            || !request.Password.Any(char.IsDigit))
        {
            AddError("password", $"Password must be at least {MinPasswordLength} characters and include both letters and numbers.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            AddError("confirmPassword", "Passwords do not match.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Trim().Length > User.MaxNameLength)
        {
            AddError("firstName", $"First name is required and must be {User.MaxNameLength} characters or fewer.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Trim().Length > User.MaxNameLength)
        {
            AddError("lastName", $"Last name is required and must be {User.MaxNameLength} characters or fewer.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.DateOfBirth > today)
        {
            AddError("dateOfBirth", "Date of birth cannot be in the future.");
        }
        else if (GetAge(request.DateOfBirth, today) < User.MinAgeYears)
        {
            AddError("dateOfBirth", $"You must be at least {User.MinAgeYears} years old to register.");
        }

        if (string.IsNullOrWhiteSpace(request.Location) || request.Location.Trim().Length > User.MaxLocationLength)
        {
            AddError("location", $"Location is required and must be {User.MaxLocationLength} characters or fewer.");
        }

        return errors.ToDictionary(e => e.Key, e => e.Value.ToArray());
    }

    private int GetAge(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }
}
