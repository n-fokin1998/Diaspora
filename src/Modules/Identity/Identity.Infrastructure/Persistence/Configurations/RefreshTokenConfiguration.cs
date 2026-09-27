using Diaspora.Identity.Domain.RefreshTokens;
using Diaspora.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diaspora.Identity.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.HasIndex(t => t.TokenHash).HasDatabaseName("ix_refresh_tokens_token_hash").IsUnique();

        builder.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamptz").IsRequired();
        builder.Property(t => t.ExpiresAtUtc).HasColumnName("expires_at_utc").HasColumnType("timestamptz").IsRequired();
        builder.Property(t => t.RevokedAtUtc).HasColumnName("revoked_at_utc").HasColumnType("timestamptz");
        builder.Property(t => t.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash").HasMaxLength(64);

        builder.Ignore(t => t.IsActive);
    }
}
