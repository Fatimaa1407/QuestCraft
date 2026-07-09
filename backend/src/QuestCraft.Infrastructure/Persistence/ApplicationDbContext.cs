using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Common;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserStatistics> UserStatistics => Set<UserStatistics>();

    public DbSet<ChallengeCategory> ChallengeCategories => Set<ChallengeCategory>();
    public DbSet<ChallengeDifficulty> ChallengeDifficulties => Set<ChallengeDifficulty>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<HiddenTestCase> HiddenTestCases => Set<HiddenTestCase>();
    public DbSet<ChallengeSubmission> ChallengeSubmissions => Set<ChallengeSubmission>();
    public DbSet<SubmissionResult> SubmissionResults => Set<SubmissionResult>();

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<DailyQuestTemplate> DailyQuestTemplates => Set<DailyQuestTemplate>();
    public DbSet<UserDailyQuest> UserDailyQuests => Set<UserDailyQuest>();
    public DbSet<Streak> Streaks => Set<Streak>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<LeaderboardSnapshot> LeaderboardSnapshots => Set<LeaderboardSnapshot>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<XpTransaction> XpTransactions => Set<XpTransaction>();

    public DbSet<MarketplaceItemType> MarketplaceItemTypes => Set<MarketplaceItemType>();
    public DbSet<MarketplaceItem> MarketplaceItems => Set<MarketplaceItem>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<ChallengeHint> ChallengeHints => Set<ChallengeHint>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<ExcelImportLog> ExcelImportLogs => Set<ExcelImportLog>();
    public DbSet<RateLimitLog> RateLimitLogs => Set<RateLimitLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global soft-delete filter for every entity that derives from BaseEntity.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
