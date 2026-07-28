using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();

        // SetNull so audit history survives even if the acting user is later removed.
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.Property(s => s.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.Key).IsUnique();
    }
}

public class RateLimitLogConfiguration : IEntityTypeConfiguration<RateLimitLog>
{
    public void Configure(EntityTypeBuilder<RateLimitLog> builder)
    {
        builder.Property(r => r.IpAddress).HasMaxLength(45).IsRequired();
        builder.Property(r => r.Endpoint).HasMaxLength(200).IsRequired();

        builder.HasIndex(r => new { r.IpAddress, r.Endpoint, r.WindowStart });
    }
}
