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

public class MarketplaceBundleConfiguration : IEntityTypeConfiguration<MarketplaceBundle>
{
    public void Configure(EntityTypeBuilder<MarketplaceBundle> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(150).IsRequired();
    }
}

public class MarketplaceBundleItemConfiguration : IEntityTypeConfiguration<MarketplaceBundleItem>
{
    public void Configure(EntityTypeBuilder<MarketplaceBundleItem> builder)
    {
        builder.HasOne(bi => bi.Bundle)
            .WithMany(b => b.Items)
            .HasForeignKey(bi => bi.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bi => bi.MarketplaceItem)
            .WithMany()
            .HasForeignKey(bi => bi.MarketplaceItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bi => new { bi.BundleId, bi.MarketplaceItemId }).IsUnique();
    }
}

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.MarketplaceItem)
            .WithMany()
            .HasForeignKey(w => w.MarketplaceItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.UserId, w.MarketplaceItemId }).IsUnique();
    }
}

