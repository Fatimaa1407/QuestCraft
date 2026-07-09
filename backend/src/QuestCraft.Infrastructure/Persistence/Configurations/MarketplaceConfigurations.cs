using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Infrastructure.Persistence.Configurations;

public class MarketplaceItemTypeConfiguration : IEntityTypeConfiguration<MarketplaceItemType>
{
    public void Configure(EntityTypeBuilder<MarketplaceItemType> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class MarketplaceItemConfiguration : IEntityTypeConfiguration<MarketplaceItem>
{
    public void Configure(EntityTypeBuilder<MarketplaceItem> builder)
    {
        builder.Property(i => i.Name).HasMaxLength(150).IsRequired();

        builder.HasOne(i => i.ItemType)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.ItemTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasOne(p => p.User)
            .WithMany(u => u.Purchases)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.MarketplaceItem)
            .WithMany(i => i.Purchases)
            .HasForeignKey(p => p.MarketplaceItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChallengeHintConfiguration : IEntityTypeConfiguration<ChallengeHint>
{
    public void Configure(EntityTypeBuilder<ChallengeHint> builder)
    {
        builder.HasIndex(h => new { h.UserId, h.ChallengeId }).IsUnique();

        builder.HasOne(h => h.User)
            .WithMany(u => u.UnlockedHints)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Challenge)
            .WithMany(c => c.UnlockedHints)
            .HasForeignKey(h => h.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
