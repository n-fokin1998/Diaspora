using Diaspora.Identity.Domain.RefreshTokens;
using Diaspora.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Diaspora.Identity.Infrastructure.Persistence;

internal class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
