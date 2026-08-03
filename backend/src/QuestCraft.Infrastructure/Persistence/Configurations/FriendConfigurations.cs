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
        // Not required any more — a message can be image-only (SendChatMessageCommandValidator
        // enforces that at least one of Content/ImageDataUrl is present). ImageDataUrl is left
        // with no HasMaxLength/HasColumnType — EF's SQL Server convention already maps an
        // unbounded string to nvarchar(max) on its own, and an explicit "nvarchar(max)" column
        // type string is SQL-Server-specific syntax that breaks the SQLite provider the
        // integration tests run against.
        builder.Property(m => m.Content).HasMaxLength(2000).IsRequired(false);
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
