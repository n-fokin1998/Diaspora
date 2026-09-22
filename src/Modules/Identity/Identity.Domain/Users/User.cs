namespace Diaspora.Identity.Domain.Users;

public class User
{
    public const int MinAgeYears = 13;
    public const int MaxNameLength = 100;
    public const int MaxLocationLength = 200;

    private User()
    {
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public byte[] PasswordHash { get; private set; } = [];
    public byte[] PasswordSalt { get; private set; } = [];
    public int PasswordHashIterations { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    public static User Register(
        string email,
        byte[] passwordHash,
        byte[] passwordSalt,
        int passwordHashIterations,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string location,
        DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        if (passwordHash.Length == 0)
        {
            throw new ArgumentException("Password hash must not be empty.", nameof(passwordHash));
        }
        if (passwordSalt.Length == 0)
        {
            throw new ArgumentException("Password salt must not be empty.", nameof(passwordSalt));
        }
        if (dateOfBirth > DateOnly.FromDateTime(createdAtUtc))
        {
            throw new ArgumentException("Date of birth must not be in the future.", nameof(dateOfBirth));
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            NormalizedEmail = email.NormalizeEmail(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            PasswordHashIterations = passwordHashIterations,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            DateOfBirth = dateOfBirth,
            Location = location.Trim(),
            CreatedAtUtc = createdAtUtc,
        };
    }
}
