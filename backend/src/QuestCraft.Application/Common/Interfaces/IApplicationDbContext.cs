using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }

    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<UserStatistics> UserStatistics { get; }

    DbSet<ChallengeCategory> ChallengeCategories { get; }
    DbSet<ChallengeDifficulty> ChallengeDifficulties { get; }
    DbSet<Challenge> Challenges { get; }
    DbSet<TestCase> TestCases { get; }
    DbSet<HiddenTestCase> HiddenTestCases { get; }
    DbSet<ChallengeSubmission> ChallengeSubmissions { get; }
    DbSet<SubmissionResult> SubmissionResults { get; }

    DbSet<Quiz> Quizzes { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }
    DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; }

    DbSet<Achievement> Achievements { get; }
    DbSet<UserAchievement> UserAchievements { get; }
    DbSet<DailyQuestTemplate> DailyQuestTemplates { get; }
    DbSet<UserDailyQuest> UserDailyQuests { get; }
    DbSet<Streak> Streaks { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<LeaderboardSnapshot> LeaderboardSnapshots { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<XpTransaction> XpTransactions { get; }

    DbSet<MarketplaceItemType> MarketplaceItemTypes { get; }
    DbSet<MarketplaceItem> MarketplaceItems { get; }
    DbSet<Purchase> Purchases { get; }
    DbSet<ChallengeHint> ChallengeHints { get; }

    DbSet<AuditLog> AuditLogs { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<ExcelImportLog> ExcelImportLogs { get; }
    DbSet<RateLimitLog> RateLimitLogs { get; }
    DbSet<SeasonalEvent> SeasonalEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
