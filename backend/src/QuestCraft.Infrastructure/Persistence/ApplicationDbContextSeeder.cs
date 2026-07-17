using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Infrastructure.Persistence;

public static class ApplicationDbContextSeeder
{
    public const string DefaultAdminEmail = "admin@questcraft.local";
    public const string DefaultAdminPassword = "Admin@12345";

    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        var adminRole = await SeedRolesAsync(context);
        await SeedAdminUserAsync(context, adminRole, passwordHasher);
        await SeedChallengeDifficultiesAsync(context);
        await SeedChallengeCategoriesAsync(context);
        await SeedMarketplaceItemTypesAsync(context);
        await SeedMarketplaceItemsAsync(context);
        await SeedDailyQuestTemplatesAsync(context);
        await SeedAchievementsAsync(context);
        await SeedSystemSettingsAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task<Role> SeedRolesAsync(ApplicationDbContext context)
    {
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null)
        {
            adminRole = new Role { Name = "Admin" };
            context.Roles.Add(adminRole);
        }

        if (!await context.Roles.AnyAsync(r => r.Name == "Student"))
        {
            context.Roles.Add(new Role { Name = "Student" });
        }

        await context.SaveChangesAsync();
        return adminRole;
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext context, Role adminRole, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync(u => u.Email == DefaultAdminEmail))
        {
            return;
        }

        var admin = new User
        {
            Username = "admin",
            FirstName = "Admin",
            LastName = "QuestCraft",
            Email = DefaultAdminEmail,
            PasswordHash = passwordHasher.Hash(DefaultAdminPassword),
            RoleId = adminRole.Id,
            IsActive = true,
            Profile = new UserProfile(),
            Statistics = new UserStatistics(),
            Streak = new Streak(),
        };

        context.Users.Add(admin);
    }

    private static async Task SeedChallengeDifficultiesAsync(ApplicationDbContext context)
    {
        if (await context.ChallengeDifficulties.AnyAsync())
        {
            return;
        }

        context.ChallengeDifficulties.AddRange(
            new ChallengeDifficulty { Name = "Easy", Color = "#22c55e", XpMultiplier = 1.0 },
            new ChallengeDifficulty { Name = "Medium", Color = "#f59e0b", XpMultiplier = 1.5 },
            new ChallengeDifficulty { Name = "Hard", Color = "#ef4444", XpMultiplier = 2.0 }
        );
    }

    private static async Task SeedChallengeCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.ChallengeCategories.AnyAsync())
        {
            return;
        }

        string[] categories = ["Arrays", "Strings", "Loops & Conditions", "OOP", "LINQ", "Collections"];
        context.ChallengeCategories.AddRange(categories.Select(name => new ChallengeCategory { Name = name }));
    }

    private static async Task SeedMarketplaceItemTypesAsync(ApplicationDbContext context)
    {
        if (await context.MarketplaceItemTypes.AnyAsync())
        {
            return;
        }

        string[] types = ["Hint", "Avatar", "ProfileFrame", "Theme", "Badge", "Title", "StreakFreeze"];
        context.MarketplaceItemTypes.AddRange(types.Select(name => new MarketplaceItemType { Name = name }));

        // Committed immediately so SeedMarketplaceItemsAsync can look these up by real Id right after.
        await context.SaveChangesAsync();
    }

    private static async Task SeedMarketplaceItemsAsync(ApplicationDbContext context)
    {
        if (await context.MarketplaceItems.AnyAsync())
        {
            return;
        }

        var frameTypeId = await context.MarketplaceItemTypes.Where(t => t.Name == "ProfileFrame").Select(t => t.Id).FirstAsync();
        var titleTypeId = await context.MarketplaceItemTypes.Where(t => t.Name == "Title").Select(t => t.Id).FirstAsync();
        var themeTypeId = await context.MarketplaceItemTypes.Where(t => t.Name == "Theme").Select(t => t.Id).FirstAsync();
        var badgeTypeId = await context.MarketplaceItemTypes.Where(t => t.Name == "Badge").Select(t => t.Id).FirstAsync();
        var streakFreezeTypeId = await context.MarketplaceItemTypes.Where(t => t.Name == "StreakFreeze").Select(t => t.Id).FirstAsync();

        context.MarketplaceItems.AddRange(
            new MarketplaceItem { Name = "Qızıl Çərçivə", ItemTypeId = frameTypeId, Price = 100 },
            new MarketplaceItem { Name = "Kod Ustası", ItemTypeId = titleTypeId, Price = 150 },
            new MarketplaceItem { Name = "Tünd Tema", ItemTypeId = themeTypeId, Price = 80 },
            new MarketplaceItem { Name = "İlk Addım Nişanı", ItemTypeId = badgeTypeId, Price = 30 },
            new MarketplaceItem
            {
                Name = "Streak Dondurma",
                NameEn = "Streak Freeze",
                Description = "Bir gün fəaliyyətsiz qalsanız belə ardıcıllığınızı qorumaq üçün birdəfəlik daimi qoruma.",
                DescriptionEn = "A one-time, permanent safeguard that protects your streak if you miss a single day.",
                ItemTypeId = streakFreezeTypeId,
                Price = 120,
            }
        );
    }

    private static async Task SeedDailyQuestTemplatesAsync(ApplicationDbContext context)
    {
        if (await context.DailyQuestTemplates.AnyAsync())
        {
            return;
        }

        context.DailyQuestTemplates.AddRange(
            new DailyQuestTemplate
            {
                Title = "3 Challenge həll et",
                TargetType = DailyQuestTargetType.SolveChallenge,
                TargetValue = 3,
                XpReward = 50,
                CoinReward = 20,
            },
            new DailyQuestTemplate
            {
                Title = "1 Quiz tamamla",
                TargetType = DailyQuestTargetType.CompleteQuiz,
                TargetValue = 1,
                XpReward = 30,
                CoinReward = 10,
            },
            new DailyQuestTemplate
            {
                Title = "100 XP qazan",
                TargetType = DailyQuestTargetType.EarnXp,
                TargetValue = 100,
                XpReward = 40,
                CoinReward = 15,
            }
        );
    }

    private static async Task SeedAchievementsAsync(ApplicationDbContext context)
    {
        if (await context.Achievements.AnyAsync())
        {
            return;
        }

        context.Achievements.AddRange(
            new Achievement
            {
                Name = "First Challenge",
                Description = "İlk challenge-ə cəhd etdin.",
                ConditionType = AchievementConditionType.SubmissionCount,
                ConditionValue = 1,
                XpReward = 10,
                CoinReward = 5,
            },
            new Achievement
            {
                Name = "First Accepted Solution",
                Description = "İlk challenge-ini uğurla həll etdin.",
                ConditionType = AchievementConditionType.AcceptedCount,
                ConditionValue = 1,
                XpReward = 20,
                CoinReward = 10,
            },
            new Achievement
            {
                Name = "10 Challenges Completed",
                Description = "10 challenge həll etdin.",
                ConditionType = AchievementConditionType.AcceptedCount,
                ConditionValue = 10,
                XpReward = 100,
                CoinReward = 50,
            },
            new Achievement
            {
                Name = "100 XP",
                Description = "Cəmi 100 XP topladın.",
                ConditionType = AchievementConditionType.XpTotal,
                ConditionValue = 100,
                XpReward = 20,
                CoinReward = 10,
            },
            new Achievement
            {
                Name = "1000 XP",
                Description = "Cəmi 1000 XP topladın.",
                ConditionType = AchievementConditionType.XpTotal,
                ConditionValue = 1000,
                XpReward = 100,
                CoinReward = 50,
            },
            new Achievement
            {
                Name = "7 Day Streak",
                Description = "7 gün ardıcıl aktiv oldun.",
                ConditionType = AchievementConditionType.StreakDays,
                ConditionValue = 7,
                XpReward = 70,
                CoinReward = 30,
            },
            new Achievement
            {
                Name = "30 Day Streak",
                Description = "30 gün ardıcıl aktiv oldun.",
                ConditionType = AchievementConditionType.StreakDays,
                ConditionValue = 30,
                XpReward = 300,
                CoinReward = 150,
            }
        );
    }

    private static async Task SeedSystemSettingsAsync(ApplicationDbContext context)
    {
        if (await context.SystemSettings.AnyAsync())
        {
            return;
        }

        context.SystemSettings.AddRange(
            new SystemSetting { Key = "Xp.LevelFormulaBase", Value = "100", Description = "RequiredXp(level) = Base * level^1.5" },
            new SystemSetting { Key = "Gamification.FirstSolveOnlyReward", Value = "true", Description = "Yalnız ilk uğurlu submission-da XP/Coin verilir" },
            new SystemSetting { Key = "Gamification.ComboBonusPercent", Value = "10", Description = "Eyni gündə 3+ ardıcıl həll üçün bonus faizi" }
        );
    }
}
