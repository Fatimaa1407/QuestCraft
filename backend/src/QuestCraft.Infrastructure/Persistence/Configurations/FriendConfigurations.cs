using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Infrastructure.Persistence.Configurations;

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(f => new { f.RequesterId, f.AddresseeId }).IsUnique();

        // Both FKs point at Users — SQL Server rejects cascade paths that could reach the same
        // row twice, so both sides must be Restrict (friend requests are cleaned up manually,
        // same pattern as every other "reference data" FK in this codebase).
        builder.HasOne(f => f.Requester)
            .WithMany()
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Addressee)
            .WithMany()
            .HasForeignKey(f => f.AddresseeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.Property(m => m.Content).HasMaxLength(2000).IsRequired();
        builder.HasIndex(m => new { m.SenderId, m.RecipientId, m.CreatedAt });

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
