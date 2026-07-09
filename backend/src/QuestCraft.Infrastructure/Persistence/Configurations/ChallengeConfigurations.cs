using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Infrastructure.Persistence.Configurations;

public class ChallengeCategoryConfiguration : IEntityTypeConfiguration<ChallengeCategory>
{
    public void Configure(EntityTypeBuilder<ChallengeCategory> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();
    }
}

public class ChallengeDifficultyConfiguration : IEntityTypeConfiguration<ChallengeDifficulty>
{
    public void Configure(EntityTypeBuilder<ChallengeDifficulty> builder)
    {
        builder.Property(d => d.Name).HasMaxLength(30).IsRequired();
        builder.HasIndex(d => d.Name).IsUnique();
    }
}

public class ChallengeConfiguration : IEntityTypeConfiguration<Challenge>
{
    public void Configure(EntityTypeBuilder<Challenge> builder)
    {
        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.Property(c => c.StarterCode).IsRequired();

        builder.HasOne(c => c.Category)
            .WithMany(cat => cat.Challenges)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Difficulty)
            .WithMany(d => d.Challenges)
            .HasForeignKey(c => c.DifficultyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TestCaseConfiguration : IEntityTypeConfiguration<TestCase>
{
    public void Configure(EntityTypeBuilder<TestCase> builder)
    {
        builder.HasOne(t => t.Challenge)
            .WithMany(c => c.TestCases)
            .HasForeignKey(t => t.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HiddenTestCaseConfiguration : IEntityTypeConfiguration<HiddenTestCase>
{
    public void Configure(EntityTypeBuilder<HiddenTestCase> builder)
    {
        builder.HasOne(t => t.Challenge)
            .WithMany(c => c.HiddenTestCases)
            .HasForeignKey(t => t.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChallengeSubmissionConfiguration : IEntityTypeConfiguration<ChallengeSubmission>
{
    public void Configure(EntityTypeBuilder<ChallengeSubmission> builder)
    {
        builder.Property(s => s.SourceCode).IsRequired();
        builder.Property(s => s.Verdict).HasConversion<string>().HasMaxLength(30);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Challenge)
            .WithMany(c => c.Submissions)
            .HasForeignKey(s => s.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.UserId, s.ChallengeId });
    }
}

public class SubmissionResultConfiguration : IEntityTypeConfiguration<SubmissionResult>
{
    public void Configure(EntityTypeBuilder<SubmissionResult> builder)
    {
        builder.HasOne(r => r.Submission)
            .WithMany(s => s.Results)
            .HasForeignKey(r => r.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
