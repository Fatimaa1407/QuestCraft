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

        string[] types = ["Hint", "Avatar", "ProfileFrame", "ProfileBanner", "Theme", "Badge", "Title", "StreakFreeze"];
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
        var avatarTypeId = await context.MarketplaceItemTypes.Where(t => t.Name == "Avatar").Select(t => t.Id).FirstAsync();
        var bannerTypeId = await context.MarketplaceItemTypes.Where(t => t.Name == "ProfileBanner").Select(t => t.Id).FirstAsync();

        const string robotAvatarSvg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgiIGhlaWdodD0iMTI4IiB2aWV3Qm94PSIwIDAgMTI4IDEyOCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMSI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjM2I4MmY2Ii8+PHN0b3Agb2Zmc2V0PSIxMDAlIiBzdG9wLWNvbG9yPSIjMDZiNmQ0Ii8+CiAgPC9saW5lYXJHcmFkaWVudD48L2RlZnM+CiAgPGNpcmNsZSBjeD0iNjQiIGN5PSI2NCIgcj0iNjQiIGZpbGw9InVybCgjZykiLz4KICA8dGV4dCB4PSI2NCIgeT0iODIiIGZvbnQtc2l6ZT0iNTYiIHRleHQtYW5jaG9yPSJtaWRkbGUiPvCfpJY8L3RleHQ+Cjwvc3ZnPg==";
        const string astroAvatarSvg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgiIGhlaWdodD0iMTI4IiB2aWV3Qm94PSIwIDAgMTI4IDEyOCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMSI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjOGI1Y2Y2Ii8+PHN0b3Agb2Zmc2V0PSIxMDAlIiBzdG9wLWNvbG9yPSIjZWM0ODk5Ii8+CiAgPC9saW5lYXJHcmFkaWVudD48L2RlZnM+CiAgPGNpcmNsZSBjeD0iNjQiIGN5PSI2NCIgcj0iNjQiIGZpbGw9InVybCgjZykiLz4KICA8dGV4dCB4PSI2NCIgeT0iODIiIGZvbnQtc2l6ZT0iNTYiIHRleHQtYW5jaG9yPSJtaWRkbGUiPvCfmoA8L3RleHQ+Cjwvc3ZnPg==";
        const string fireAvatarSvg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgiIGhlaWdodD0iMTI4IiB2aWV3Qm94PSIwIDAgMTI4IDEyOCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMSI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjZjk3MzE2Ii8+PHN0b3Agb2Zmc2V0PSIxMDAlIiBzdG9wLWNvbG9yPSIjZWY0NDQ0Ii8+CiAgPC9saW5lYXJHcmFkaWVudD48L2RlZnM+CiAgPGNpcmNsZSBjeD0iNjQiIGN5PSI2NCIgcj0iNjQiIGZpbGw9InVybCgjZykiLz4KICA8dGV4dCB4PSI2NCIgeT0iODIiIGZvbnQtc2l6ZT0iNTYiIHRleHQtYW5jaG9yPSJtaWRkbGUiPvCflKU8L3RleHQ+Cjwvc3ZnPg==";
        const string nightBannerSvg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI4MDAiIGhlaWdodD0iMjAwIiB2aWV3Qm94PSIwIDAgODAwIDIwMCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMCI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjMGYxNzJhIi8+PHN0b3Agb2Zmc2V0PSI1MCUiIHN0b3AtY29sb3I9IiMzMTJlODEiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiMxZTFiNGIiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8cmVjdCB3aWR0aD0iODAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0idXJsKCNnKSIvPgo8L3N2Zz4=";
        const string sunriseBannerSvg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI4MDAiIGhlaWdodD0iMjAwIiB2aWV3Qm94PSIwIDAgODAwIDIwMCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMCI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjZjk3MzE2Ii8+PHN0b3Agb2Zmc2V0PSI1MCUiIHN0b3AtY29sb3I9IiNmNDNmNWUiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiNlYzQ4OTkiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8cmVjdCB3aWR0aD0iODAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0idXJsKCNnKSIvPgo8L3N2Zz4=";
        const string forestBannerSvg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI4MDAiIGhlaWdodD0iMjAwIiB2aWV3Qm94PSIwIDAgODAwIDIwMCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMCI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjMTQ1MzJkIi8+PHN0b3Agb2Zmc2V0PSI1MCUiIHN0b3AtY29sb3I9IiMxNmEzNGEiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiMyMmM1NWUiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8cmVjdCB3aWR0aD0iODAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0idXJsKCNnKSIvPgo8L3N2Zz4=";
        const string goldenFrameSvg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgiIGhlaWdodD0iMTI4IiB2aWV3Qm94PSIwIDAgMTI4IDEyOCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMSI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjZmRlNjhhIi8+PHN0b3Agb2Zmc2V0PSI1MCUiIHN0b3AtY29sb3I9IiNmNTllMGIiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiNiNDUzMDkiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8Y2lyY2xlIGN4PSI2NCIgY3k9IjY0IiByPSI1OCIgZmlsbD0ibm9uZSIgc3Ryb2tlPSJ1cmwoI2cpIiBzdHJva2Utd2lkdGg9IjciLz4KICA8Y2lyY2xlIGN4PSI2NCIgY3k9IjYiIHI9IjQiIGZpbGw9IiNmZGU2OGEiLz4KICA8Y2lyY2xlIGN4PSI2NCIgY3k9IjEyMiIgcj0iNCIgZmlsbD0iI2I0NTMwOSIvPgogIDxjaXJjbGUgY3g9IjYiIGN5PSI2NCIgcj0iNCIgZmlsbD0iI2Y1OWUwYiIvPgogIDxjaXJjbGUgY3g9IjEyMiIgY3k9IjY0IiByPSI0IiBmaWxsPSIjZjU5ZTBiIi8+Cjwvc3ZnPg==";
        const string firstStepBadgeSvg = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI2NCIgaGVpZ2h0PSI2NCIgdmlld0JveD0iMCAwIDY0IDY0Ij4KICA8ZGVmcz48bGluZWFyR3JhZGllbnQgaWQ9ImciIHgxPSIwIiB5MT0iMCIgeDI9IjEiIHkyPSIxIj4KICAgIDxzdG9wIG9mZnNldD0iMCUiIHN0b3AtY29sb3I9IiMzNGQzOTkiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiMwNTk2NjkiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8Y2lyY2xlIGN4PSIzMiIgY3k9IjMyIiByPSIzMCIgZmlsbD0idXJsKCNnKSIvPgogIDx0ZXh0IHg9IjMyIiB5PSI0MiIgZm9udC1zaXplPSIyOCIgdGV4dC1hbmNob3I9Im1pZGRsZSI+8J+RozwvdGV4dD4KPC9zdmc+";

        context.MarketplaceItems.AddRange(
            new MarketplaceItem { Name = "Qızıl Çərçivə", ItemTypeId = frameTypeId, Price = 100, ImageUrl = goldenFrameSvg },
            new MarketplaceItem { Name = "Kod Ustası", ItemTypeId = titleTypeId, Price = 150 },
            new MarketplaceItem { Name = "Tünd Tema", ItemTypeId = themeTypeId, Price = 80 },
            new MarketplaceItem { Name = "İlk Addım Nişanı", ItemTypeId = badgeTypeId, Price = 30, ImageUrl = firstStepBadgeSvg },
            new MarketplaceItem
            {
                Name = "Streak Dondurma",
                NameEn = "Streak Freeze",
                Description = "Bir gün fəaliyyətsiz qalsanız belə ardıcıllığınızı qorumaq üçün birdəfəlik daimi qoruma.",
                DescriptionEn = "A one-time, permanent safeguard that protects your streak if you miss a single day.",
                ItemTypeId = streakFreezeTypeId,
                Price = 120,
            },
            new MarketplaceItem { Name = "Robot Avatarı", NameEn = "Robot Avatar", Description = "Futuristik robot avatarı.", DescriptionEn = "A futuristic robot avatar.", ItemTypeId = avatarTypeId, Price = 60, ImageUrl = robotAvatarSvg },
            new MarketplaceItem { Name = "Astronavt Avatarı", NameEn = "Astronaut Avatar", Description = "Kosmosu fəth edən astronavt avatarı.", DescriptionEn = "A space-conquering astronaut avatar.", ItemTypeId = avatarTypeId, Price = 90, ImageUrl = astroAvatarSvg },
            new MarketplaceItem { Name = "Alov Avatarı", NameEn = "Flame Avatar", Description = "Alovlu bir avatar.", DescriptionEn = "A fiery avatar.", ItemTypeId = avatarTypeId, Price = 90, ImageUrl = fireAvatarSvg },
            new MarketplaceItem { Name = "Gecə Səması Banneri", NameEn = "Night Sky Banner", Description = "Profil başlığını ulduzlu gecə rənginə boyayır.", DescriptionEn = "Paints your profile header in starry-night colors.", ItemTypeId = bannerTypeId, Price = 70, ImageUrl = nightBannerSvg },
            new MarketplaceItem { Name = "Gündoğuşu Banneri", NameEn = "Sunrise Banner", Description = "İlıq narıncı-çəhrayı gündoğuşu rəngləri.", DescriptionEn = "Warm orange-pink sunrise colors.", ItemTypeId = bannerTypeId, Price = 70, ImageUrl = sunriseBannerSvg },
            new MarketplaceItem { Name = "Meşə Banneri", NameEn = "Forest Banner", Description = "Sərin, canlı meşə yaşılı tonları.", DescriptionEn = "Cool, vivid forest-green tones.", ItemTypeId = bannerTypeId, Price = 70, ImageUrl = forestBannerSvg },
            new MarketplaceItem { Name = "Bənövşəyi Tema", NameEn = "Violet Theme", Description = "Dashboard aksent rənglərini bənövşəyi tonlara dəyişir.", DescriptionEn = "Changes the dashboard accent colors to violet tones.", ItemTypeId = themeTypeId, Price = 80 },
            new MarketplaceItem { Name = "Narıncı Tema", NameEn = "Sunset Theme", Description = "Dashboard aksent rənglərini isti narıncı tonlara dəyişir.", DescriptionEn = "Changes the dashboard accent colors to warm orange tones.", ItemTypeId = themeTypeId, Price = 80 }
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
