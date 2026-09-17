using Identity.Domain.Users;

namespace Diaspora.Tests.Modules.Identity.Domain.Users;

public class UserTests
{
    private static readonly byte[] SomeHash = [1, 2, 3];
    private static readonly byte[] SomeSalt = [4, 5, 6];
    private static readonly DateTime Now = new(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Register_WithValidInput_ReturnsUser()
    {
        var user = User.Register(
            email: "Jane@Example.com",
            passwordHash: SomeHash,
            passwordSalt: SomeSalt,
            passwordHashIterations: 210_000,
            firstName: "Jane",
            lastName: "Doe",
            dateOfBirth: new DateOnly(1998, 4, 12),
            location: "Berlin, Germany",
            createdAtUtc: Now);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Jane@Example.com", user.Email);
        Assert.Equal("JANE@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("Berlin, Germany", user.Location);
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData(" ", "Doe")]
    [InlineData("Jane", "")]
    [InlineData("Jane", " ")]
    public void Register_WithEmptyOrWhitespaceName_Throws(string firstName, string lastName)
    {
        Assert.Throws<ArgumentException>(() => User.Register(
            "jane@example.com", SomeHash, SomeSalt, 210_000, firstName, lastName,
            new DateOnly(1998, 4, 12), "Berlin", Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithEmptyOrWhitespaceLocation_Throws(string location)
    {
        Assert.Throws<ArgumentException>(() => User.Register(
            "jane@example.com", SomeHash, SomeSalt, 210_000, "Jane", "Doe",
            new DateOnly(1998, 4, 12), location, Now));
    }

    [Fact]
    public void Register_WithFutureDateOfBirth_Throws()
    {
        var futureDate = DateOnly.FromDateTime(Now).AddDays(1);

        Assert.Throws<ArgumentException>(() => User.Register(
            "jane@example.com", SomeHash, SomeSalt, 210_000, "Jane", "Doe",
            futureDate, "Berlin", Now));
    }

    [Fact]
    public void Register_WithEmptyPasswordHash_Throws()
    {
        Assert.Throws<ArgumentException>(() => User.Register(
            "jane@example.com", [], SomeSalt, 210_000, "Jane", "Doe",
            new DateOnly(1998, 4, 12), "Berlin", Now));
    }

    [Fact]
    public void Register_WithEmptyPasswordSalt_Throws()
    {
        Assert.Throws<ArgumentException>(() => User.Register(
            "jane@example.com", SomeHash, [], 210_000, "Jane", "Doe",
            new DateOnly(1998, 4, 12), "Berlin", Now));
    }
}
