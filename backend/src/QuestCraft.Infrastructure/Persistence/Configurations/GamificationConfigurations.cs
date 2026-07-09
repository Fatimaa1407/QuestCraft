using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Infrastructure.Persistence.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(150).IsRequired();
        builder.Property(a => a.ConditionType).HasConversion<string>().HasMaxLength(30);
    }
}

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();

        builder.HasOne(ua => ua.User)
            .WithMany(u => u.Achievements)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Achievement)
            .WithMany(a => a.UnlockedBy)
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DailyQuestTemplateConfiguration : IEntityTypeConfiguration<DailyQuestTemplate>
{
    public void Configure(EntityTypeBuilder<DailyQuestTemplate> builder)
    {
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.TargetType).HasConversion<string>().HasMaxLength(30);
    }
}

public class UserDailyQuestConfiguration : IEntityTypeConfiguration<UserDailyQuest>
{
    public void Configure(EntityTypeBuilder<UserDailyQuest> builder)
    {
        builder.HasIndex(q => new { q.UserId, q.DailyQuestTemplateId, q.QuestDate }).IsUnique();

        builder.HasOne(q => q.User)
            .WithMany(u => u.DailyQuests)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.DailyQuestTemplate)
            .WithMany(t => t.Instances)
            .HasForeignKey(q => q.DailyQuestTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StreakConfiguration : IEntityTypeConfiguration<Streak>
{
    public void Configure(EntityTypeBuilder<Streak> builder)
    {
        builder.HasIndex(s => s.UserId).IsUnique();

        builder.HasOne(s => s.User)
            .WithOne(u => u.Streak)
            .HasForeignKey<Streak>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasIndex(a => new { a.UserId, a.ActivityDate }).IsUnique();

        builder.HasOne(a => a.User)
            .WithMany(u => u.ActivityLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LeaderboardSnapshotConfiguration : IEntityTypeConfiguration<LeaderboardSnapshot>
{
    public void Configure(EntityTypeBuilder<LeaderboardSnapshot> builder)
    {
        builder.Property(l => l.Period).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(l => new { l.Period, l.SnapshotDate, l.Rank });

        builder.HasOne(l => l.User)
            .WithMany(u => u.LeaderboardSnapshots)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
