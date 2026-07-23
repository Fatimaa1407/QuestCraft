using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Infrastructure.Persistence.Configurations;

public class ChallengeCommentConfiguration : IEntityTypeConfiguration<ChallengeComment>
{
    public void Configure(EntityTypeBuilder<ChallengeComment> builder)
    {
        builder.Property(c => c.Content).HasMaxLength(1000).IsRequired();
        builder.HasIndex(c => new { c.ChallengeId, c.CreatedAt });

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Challenge)
            .WithMany()
            .HasForeignKey(c => c.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing FK — SQL Server forbids cascade cycles here, so replies are cleaned up
        // in application code (deleting a parent with replies is disallowed, not cascaded).
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
