using Diaspora.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diaspora.Identity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(u => u.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.NormalizedEmail).HasDatabaseName("ix_users_normalized_email").IsUnique();

        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(u => u.PasswordSalt).HasColumnName("password_salt").IsRequired();
        builder.Property(u => u.PasswordHashIterations).HasColumnName("password_hash_iterations").IsRequired();

        builder.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(User.MaxNameLength).IsRequired();
        builder.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(User.MaxNameLength).IsRequired();
        builder.Property(u => u.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date").IsRequired();
        builder.Property(u => u.Location).HasColumnName("location").HasMaxLength(User.MaxLocationLength).IsRequired();

        builder.Property(u => u.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamptz").IsRequired();
    }
}
