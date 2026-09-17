using Identity.Domain.Users;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Diaspora.Tests.Modules.Identity.Infrastructure.Persistence.Repositories;

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    private IdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new IdentityDbContext(options);
    }

    private static User CreateUser(string email) => User.Register(
        email, [1, 2, 3], [4, 5, 6], 600_000,
        "Jane", "Doe", new DateOnly(1998, 4, 12), "Berlin",
        DateTime.UtcNow);

    [Fact]
    public async Task AddUser_ThenSaveChanges_PersistsAndRoundTrips()
    {
        var user = CreateUser("jane@example.com");

        await using (var writeContext = CreateContext())
        {
            var repository = new UserRepository(writeContext);
            var unitOfWork = new UnitOfWork(writeContext);

            repository.AddUser(user);
            await unitOfWork.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var persisted = await new UserRepository(readContext).FindByNormalizedEmailAsync(user.NormalizedEmail);

        Assert.NotNull(persisted);
        Assert.Equal(user.Email, persisted!.Email);
        Assert.Equal(user.NormalizedEmail, persisted.NormalizedEmail);
        Assert.Equal(user.FirstName, persisted.FirstName);
        Assert.Equal(user.LastName, persisted.LastName);
        Assert.Equal(user.Location, persisted.Location);
        Assert.Equal(user.DateOfBirth, persisted.DateOfBirth);
    }

    [Fact]
    public async Task EmailExistsAsync_WithCaseAndWhitespaceVariantOfExistingEmail_ReturnsTrue()
    {
        await using (var seedContext = CreateContext())
        {
            var repository = new UserRepository(seedContext);
            repository.AddUser(CreateUser("jane@example.com"));
            await new UnitOfWork(seedContext).SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var exists = await new UserRepository(readContext).EmailExistsAsync("  JANE@EXAMPLE.COM  ".NormalizeEmail());

        Assert.True(exists);
    }

    [Fact]
    public async Task AddUser_WithCaseAndWhitespaceVariantOfExistingEmail_ViolatesUniqueIndexOnSave()
    {
        await using (var seedContext = CreateContext())
        {
            var repository = new UserRepository(seedContext);
            repository.AddUser(CreateUser("jane@example.com"));
            await new UnitOfWork(seedContext).SaveChangesAsync();
        }

        await using var duplicateContext = CreateContext();
        new UserRepository(duplicateContext).AddUser(CreateUser("  JANE@EXAMPLE.COM  "));

        await Assert.ThrowsAsync<DbUpdateException>(
            () => new UnitOfWork(duplicateContext).SaveChangesAsync());
    }
}
