using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Infrastructure.Persistence.Configurations;

public class BattleConfiguration : IEntityTypeConfiguration<Battle>
{
    public void Configure(EntityTypeBuilder<Battle> builder)
    {
        builder.Property(b => b.Mode).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(b => b.JoinCode).IsUnique().HasFilter("[JoinCode] IS NOT NULL");

        builder.HasOne(b => b.HostUser)
            .WithMany()
            .HasForeignKey(b => b.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Challenge)
            .WithMany()
            .HasForeignKey(b => b.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.InvitedUser)
            .WithMany()
            .HasForeignKey(b => b.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BattleParticipantConfiguration : IEntityTypeConfiguration<BattleParticipant>
{
    public void Configure(EntityTypeBuilder<BattleParticipant> builder)
    {
        builder.HasIndex(p => new { p.BattleId, p.UserId }).IsUnique();

        // Cascade from Battle (the only path in) so deleting a battle cleans up its participants;
        // the User FK stays Restrict to avoid SQL Server rejecting a second cascade path to Users.
        builder.HasOne(p => p.Battle)
            .WithMany(b => b.Participants)
            .HasForeignKey(p => p.BattleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
