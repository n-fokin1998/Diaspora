using Diaspora.Identity.Domain.RefreshTokens;
using Diaspora.Identity.Domain.Users;
using Diaspora.Identity.Infrastructure.Persistence;
using Diaspora.Identity.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Diaspora.Tests.Modules.Identity.Infrastructure.Persistence;

public class RefreshTokenRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    [Fact]
    public async Task AddRefreshToken_ThenSaveChanges_PersistsAndRoundTrips()
    {
        var userId = await SeedUserAsync();
        var now = DateTime.UtcNow;
        var token = RefreshToken.Issue(userId, "some-hash", now, now.AddDays(14));

        await using (var writeContext = CreateContext())
        {
            new RefreshTokenRepository(writeContext).AddRefreshToken(token);
            await new UnitOfWork(writeContext).SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var persisted = await new RefreshTokenRepository(readContext).FindByTokenHashAsync("some-hash");

        Assert.NotNull(persisted);
        Assert.Equal(userId, persisted!.UserId);
        Assert.Null(persisted.RevokedAtUtc);
    }

    [Fact]
    public async Task RotatedToken_CanBeFoundByHash_WithReplacedByTokenHashSet()
    {
        var userId = await SeedUserAsync();
        var now = DateTime.UtcNow;
        var oldToken = RefreshToken.Issue(userId, "old-hash", now, now.AddDays(14));

        await using (var writeContext = CreateContext())
        {
            new RefreshTokenRepository(writeContext).AddRefreshToken(oldToken);
            await new UnitOfWork(writeContext).SaveChangesAsync();
        }

        await using (var rotateContext = CreateContext())
        {
            var repository = new RefreshTokenRepository(rotateContext);
            var existing = await repository.FindByTokenHashAsync("old-hash");
            existing!.Revoke(DateTime.UtcNow, "new-hash");
            await new UnitOfWork(rotateContext).SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var rotated = await new RefreshTokenRepository(readContext).FindByTokenHashAsync("old-hash");

        Assert.NotNull(rotated);
        Assert.NotNull(rotated!.RevokedAtUtc);
        Assert.Equal("new-hash", rotated.ReplacedByTokenHash);
    }

    [Fact]
    public async Task RevokeAllActiveForUserAsync_RevokesEveryActiveTokenForThatUser_ButNotOtherUsers()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync("other@example.com");
        var now = DateTime.UtcNow;

        await using (var writeContext = CreateContext())
        {
            var repository = new RefreshTokenRepository(writeContext);
            repository.AddRefreshToken(RefreshToken.Issue(userId, "hash-1", now, now.AddDays(14)));
            repository.AddRefreshToken(RefreshToken.Issue(userId, "hash-2", now, now.AddDays(14)));
            repository.AddRefreshToken(RefreshToken.Issue(otherUserId, "other-hash", now, now.AddDays(14)));
            await new UnitOfWork(writeContext).SaveChangesAsync();
        }

        await using (var revokeContext = CreateContext())
        {
            var repository = new RefreshTokenRepository(revokeContext);
            await repository.RevokeAllActiveForUserAsync(userId, DateTime.UtcNow);
            await new UnitOfWork(revokeContext).SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var repository2 = new RefreshTokenRepository(readContext);
        Assert.NotNull((await repository2.FindByTokenHashAsync("hash-1"))!.RevokedAtUtc);
        Assert.NotNull((await repository2.FindByTokenHashAsync("hash-2"))!.RevokedAtUtc);
        Assert.Null((await repository2.FindByTokenHashAsync("other-hash"))!.RevokedAtUtc);
    }

    private async Task<Guid> SeedUserAsync(string email = "jane@example.com")
    {
        var user = User.Register(
            email, [1, 2, 3], [4, 5, 6], 600_000,
            "Jane", "Doe", new DateOnly(1998, 4, 12), "Berlin",
            DateTime.UtcNow);

        await using var context = CreateContext();
        new UserRepository(context).AddUser(user);
        await new UnitOfWork(context).SaveChangesAsync();

        return user.Id;
    }

    private IdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new IdentityDbContext(options);
    }
}
