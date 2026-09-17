using Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

internal class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
