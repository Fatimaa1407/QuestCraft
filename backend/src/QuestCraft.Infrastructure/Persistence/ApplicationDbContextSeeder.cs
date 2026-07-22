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
        // Committed immediately so SeedBattlePoolChallengesAsync can look up Category/Difficulty
        // Ids by name right after (same reason SeedMarketplaceItemsAsync needs the type-seed committed first).
        await context.SaveChangesAsync();
        await SeedBattlePoolChallengesAsync(context);
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

    // Battles draw their question via BattlePoolSelector purely from Challenge rows with
    // IsBattleOnly=true — only one such row existed before this (id 63, "Battle: Ədədlərin Cəmi"),
    // meaning every duel/room battle served the exact same question. This adds 32 more,
    // deliberately kept short/easy-to-medium (stdin→stdout, single-shot, no loops over huge input)
    // since battles are timed real-time races, not leisurely practice. Guarded on a threshold
    // rather than the usual AnyAsync() so it doesn't collide with the pre-existing row.
    private static async Task SeedBattlePoolChallengesAsync(ApplicationDbContext context)
    {
        if (await context.Challenges.CountAsync(c => c.IsBattleOnly) > 5)
        {
            return;
        }

        const string starterCodeAz = "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        // Kodunuzu bura yazın\n    }\n}\n";
        const string starterCodeEn = "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        // Write your code here\n    }\n}\n";

        const int Arrays = 1, Strings = 2, LoopsConditions = 3, Oop = 4, Linq = 5, Collections = 6,
            Recursion = 7, ExceptionHandling = 8, Generics = 9, DelegatesEvents = 10, DesignPatterns = 13;
        const int Easy = 1, Medium = 2;

        Challenge Build(
            string title, string titleEn, string description, string descriptionEn,
            string inputFormat, string inputFormatEn, string outputFormat, string outputFormatEn,
            string sampleInput, string sampleOutput, int categoryId, int difficultyId, int xp, int coin,
            (string Input, string Output)[] visible, (string Input, string Output)[] hidden)
        {
            var challenge = new Challenge
            {
                Title = title,
                TitleEn = titleEn,
                Description = description,
                DescriptionEn = descriptionEn,
                InputFormat = inputFormat,
                InputFormatEn = inputFormatEn,
                OutputFormat = outputFormat,
                OutputFormatEn = outputFormatEn,
                SampleInput = sampleInput,
                SampleOutput = sampleOutput,
                StarterCode = starterCodeAz,
                StarterCodeEn = starterCodeEn,
                TimeLimitMs = 2000,
                MemoryLimitMb = 256,
                XpReward = xp,
                CoinReward = coin,
                RequiredLevel = 1,
                IsPublished = true,
                IsBattleOnly = true,
                CategoryId = categoryId,
                DifficultyId = difficultyId,
                Tags = "battle",
            };
            for (var i = 0; i < visible.Length; i++)
            {
                challenge.TestCases.Add(new TestCase { Input = visible[i].Input, ExpectedOutput = visible[i].Output, OrderIndex = i + 1 });
            }
            for (var i = 0; i < hidden.Length; i++)
            {
                challenge.HiddenTestCases.Add(new HiddenTestCase { Input = hidden[i].Input, ExpectedOutput = hidden[i].Output, OrderIndex = visible.Length + i + 1, Weight = 1 });
            }
            return challenge;
        }

        var oneLineIntsFormatAz = "Bir sətirdə boşluqla ayrılmış tam ədədlər";
        var oneLineIntsFormatEn = "Space-separated integers on one line";
        var arrayInputFormatAz = "Birinci sətirdə n, ikinci sətirdə n tam ədəd (boşluqla ayrılmış)";
        var arrayInputFormatEn = "First line n, second line n integers (space-separated)";

        context.Challenges.AddRange(
            // Arrays
            Build("Array Cəmi", "Array Sum",
                "Massivdəki bütün ədədlərin cəmini tap.", "Find the sum of all numbers in the array.",
                arrayInputFormatAz, arrayInputFormatEn, "Cəm", "The sum",
                "5\n1 2 3 4 5", "15", Arrays, Easy, 25, 8,
                [("5\n1 2 3 4 5", "15"), ("3\n10 20 30", "60")],
                [("4\n-1 -2 3 4", "4"), ("1\n7", "7")]),
            Build("Maksimum Element", "Max Element",
                "Massivdəki ən böyük elementi tap.", "Find the largest element in the array.",
                arrayInputFormatAz, arrayInputFormatEn, "Maksimum dəyər", "The maximum value",
                "4\n3 7 2 9", "9", Arrays, Easy, 25, 8,
                [("4\n3 7 2 9", "9"), ("3\n-5 -1 -10", "-1")],
                [("5\n1 1 1 1 1", "1"), ("2\n100 50", "100")]),
            Build("Array Tərsinə Çevir", "Reverse Array",
                "Massivi tərsinə çevirib çap et.", "Print the array reversed.",
                arrayInputFormatAz, arrayInputFormatEn, "Tərsinə çevrilmiş massiv (boşluqla ayrılmış)", "The reversed array (space-separated)",
                "4\n1 2 3 4", "4 3 2 1", Arrays, Medium, 35, 12,
                [("4\n1 2 3 4", "4 3 2 1"), ("3\n5 10 15", "15 10 5")],
                [("1\n9", "9"), ("5\n1 2 3 4 5", "5 4 3 2 1")]),
            Build("Cüt Ədədlərin Sayı", "Count Even Numbers",
                "Massivdəki cüt ədədlərin sayını tap.", "Count how many numbers in the array are even.",
                arrayInputFormatAz, arrayInputFormatEn, "Cüt ədədlərin sayı", "Count of even numbers",
                "5\n1 2 3 4 5", "2", Arrays, Easy, 25, 8,
                [("5\n1 2 3 4 5", "2"), ("4\n2 4 6 8", "4")],
                [("3\n1 3 5", "0"), ("6\n0 1 2 3 4 5", "3")]),

            // Strings
            Build("Sətri Tərsinə Çevir", "Reverse String",
                "Verilən sətri tərsinə çevirib çap et.", "Print the given string reversed.",
                "Bir sətir", "A single line string", "Tərsinə çevrilmiş sətir", "The reversed string",
                "hello", "olleh", Strings, Easy, 25, 8,
                [("hello", "olleh"), ("world", "dlrow")],
                [("a", "a"), ("abcd", "dcba")]),
            Build("Sait Hərflərin Sayı", "Count Vowels",
                "Sətirdəki sait hərflərin (a, e, i, o, u — böyük/kiçik fərqi olmadan) sayını tap.", "Count the vowels (a, e, i, o, u — case-insensitive) in the string.",
                "Bir sətir", "A single line string", "Sait hərflərin sayı", "Count of vowels",
                "hello", "2", Strings, Medium, 35, 12,
                [("hello", "2"), ("programming", "3")],
                [("xyz", "0"), ("AEIOU", "5")]),
            Build("Palindrom Yoxlaması", "Palindrome Check",
                "Sətrin palindrom olub-olmadığını yoxla (YES/NO çap et).", "Check whether the string is a palindrome (print YES/NO).",
                "Bir sətir", "A single line string", "YES və ya NO", "YES or NO",
                "level", "YES", Strings, Easy, 25, 8,
                [("level", "YES"), ("hello", "NO")],
                [("a", "YES"), ("abca", "NO")]),
            Build("Böyük Hərflərə Çevir", "Convert to Uppercase",
                "Sətri böyük hərflərə çevirib çap et.", "Print the string converted to uppercase.",
                "Bir sətir", "A single line string", "Böyük hərflərlə sətir", "The uppercase string",
                "hello", "HELLO", Strings, Easy, 25, 8,
                [("hello", "HELLO"), ("World", "WORLD")],
                [("abc123", "ABC123"), ("Test", "TEST")]),

            // Loops & Conditions
            Build("Faktorial", "Factorial",
                "n ədədinin faktorialını hesabla (n!).", "Compute the factorial of n (n!).",
                "Bir tam ədəd n", "A single integer n", "n!", "n!",
                "5", "120", LoopsConditions, Medium, 35, 12,
                [("5", "120"), ("0", "1")],
                [("1", "1"), ("6", "720")]),
            Build("Cüt yoxsa Tək", "Even or Odd",
                "n ədədinin cüt (EVEN) yoxsa tək (ODD) olduğunu çap et.", "Print whether n is EVEN or ODD.",
                "Bir tam ədəd n", "A single integer n", "EVEN və ya ODD", "EVEN or ODD",
                "4", "EVEN", LoopsConditions, Easy, 25, 8,
                [("4", "EVEN"), ("7", "ODD")],
                [("0", "EVEN"), ("-3", "ODD")]),
            Build("3 və ya 5-ə Bölünənlərin Cəmi", "Sum of Multiples of 3 or 5",
                "1-dən n-ə qədər 3 və ya 5-ə bölünən ədədlərin cəmini tap.", "Sum all numbers from 1 to n divisible by 3 or 5.",
                "Bir tam ədəd n", "A single integer n", "Cəm", "The sum",
                "10", "33", LoopsConditions, Medium, 35, 12,
                [("10", "33"), ("5", "8")],
                [("1", "0"), ("15", "60")]),
            Build("Rəqəmlərin Cəmi", "Digit Sum",
                "Müsbət tam ədədin rəqəmlərinin cəmini tap.", "Find the sum of the digits of a positive integer.",
                "Bir tam ədəd n", "A single integer n", "Rəqəmlərin cəmi", "Sum of the digits",
                "1234", "10", LoopsConditions, Easy, 25, 8,
                [("1234", "10"), ("99", "18")],
                [("5", "5"), ("1000", "1")]),

            // OOP
            Build("Düzbucaqlının Sahəsi", "Rectangle Area",
                "En və uzunluğa görə düzbucaqlının sahəsini tap.", "Find the area of a rectangle given width and height.",
                oneLineIntsFormatAz + " (en hündürlük)", oneLineIntsFormatEn + " (width height)", "Sahə", "The area",
                "4 5", "20", Oop, Easy, 25, 8,
                [("4 5", "20"), ("3 3", "9")],
                [("1 100", "100"), ("7 8", "56")]),
            Build("Üçbucağın Sahəsi", "Triangle Area",
                "Oturacaq və hündürlüyə görə üçbucağın sahəsini tap (tam ədəd olacaq).", "Find the area of a triangle given base and height (always a whole number).",
                oneLineIntsFormatAz + " (oturacaq hündürlük)", oneLineIntsFormatEn + " (base height)", "Sahə", "The area",
                "6 4", "12", Oop, Easy, 25, 8,
                [("6 4", "12"), ("10 3", "15")],
                [("8 8", "32"), ("2 5", "5")]),
            Build("Ən Böyük 3 Ədəddən", "Max of Three",
                "Üç ədəddən ən böyüyünü tap.", "Find the largest of three numbers.",
                oneLineIntsFormatAz, oneLineIntsFormatEn, "Ən böyük dəyər", "The largest value",
                "3 7 5", "7", Oop, Easy, 25, 8,
                [("3 7 5", "7"), ("-1 -5 -3", "-1")],
                [("10 10 10", "10"), ("0 -1 1", "1")]),

            // LINQ
            Build("Cüt Ədədlərin Cəmi", "Sum of Even Numbers",
                "Massivdəki yalnız cüt ədədlərin cəmini tap.", "Sum only the even numbers in the array.",
                arrayInputFormatAz, arrayInputFormatEn, "Cəm", "The sum",
                "5\n1 2 3 4 5", "6", Linq, Easy, 25, 8,
                [("5\n1 2 3 4 5", "6"), ("4\n10 15 20 25", "30")],
                [("3\n1 3 5", "0"), ("6\n0 1 2 3 4 5", "6")]),
            Build("Sıralanmış Massiv", "Sort Array Ascending",
                "Massivi artan sırayla sıralayıb çap et.", "Print the array sorted in ascending order.",
                arrayInputFormatAz, arrayInputFormatEn, "Sıralanmış massiv (boşluqla ayrılmış)", "The sorted array (space-separated)",
                "4\n4 1 3 2", "1 2 3 4", Linq, Medium, 35, 12,
                [("4\n4 1 3 2", "1 2 3 4"), ("3\n5 5 1", "1 5 5")],
                [("1\n9", "9"), ("5\n5 4 3 2 1", "1 2 3 4 5")]),
            Build("Unikal Elementlərin Sayı", "Count Distinct Elements",
                "Massivdəki fərqli (unikal) elementlərin sayını tap.", "Count the distinct (unique) elements in the array.",
                arrayInputFormatAz, arrayInputFormatEn, "Unikal elementlərin sayı", "Count of distinct elements",
                "5\n1 2 2 3 3", "3", Linq, Easy, 25, 8,
                [("5\n1 2 2 3 3", "3"), ("4\n7 7 7 7", "1")],
                [("1\n5", "1"), ("6\n1 2 3 4 5 6", "6")]),

            // Collections
            Build("Massivdə Axtarış", "Search in Array",
                "Massivdə x ədədinin olub-olmadığını yoxla (YES/NO çap et).", "Check whether x exists in the array (print YES/NO).",
                "n, sonra n ədəd, sonra axtarılan x", "n, then n integers, then the target x", "YES və ya NO", "YES or NO",
                "5\n1 2 3 4 5\n3", "YES", Collections, Easy, 25, 8,
                [("5\n1 2 3 4 5\n3", "YES"), ("3\n10 20 30\n25", "NO")],
                [("1\n7\n7", "YES"), ("4\n1 1 1 1\n2", "NO")]),
            Build("Ən Çox Təkrarlanan Element", "Most Frequent Element",
                "Massivdə ən çox təkrarlanan elementi tap.", "Find the most frequently occurring element in the array.",
                arrayInputFormatAz, arrayInputFormatEn, "Ən çox təkrarlanan dəyər", "The most frequent value",
                "5\n1 2 2 3 2", "2", Collections, Medium, 35, 12,
                [("5\n1 2 2 3 2", "2"), ("4\n7 7 7 8", "7")],
                [("1\n9", "9"), ("6\n1 1 2 2 2 3", "2")]),
            Build("Massivin Ortalaması", "Array Average",
                "Massivin ortalamasını tap (həmişə tam ədəd olacaq).", "Find the average of the array (always a whole number).",
                arrayInputFormatAz, arrayInputFormatEn, "Ortalama", "The average",
                "4\n2 4 6 8", "5", Collections, Medium, 35, 12,
                [("4\n2 4 6 8", "5"), ("3\n3 6 9", "6")],
                [("1\n5", "5"), ("5\n1 2 3 4 5", "3")]),

            // Recursion
            Build("Fibonacci Ədədi", "Nth Fibonacci Number",
                "n-ci Fibonacci ədədini tap (F(0)=0, F(1)=1).", "Find the nth Fibonacci number (F(0)=0, F(1)=1).",
                "Bir tam ədəd n", "A single integer n", "F(n)", "F(n)",
                "6", "8", Recursion, Medium, 35, 12,
                [("6", "8"), ("0", "0")],
                [("1", "1"), ("10", "55")]),
            Build("Ədədin Rəqəmlərinin Sayı", "Count Digits",
                "Müsbət tam ədədin neçə rəqəmdən ibarət olduğunu tap.", "Find how many digits a positive integer has.",
                "Bir tam ədəd n", "A single integer n", "Rəqəmlərin sayı", "The digit count",
                "12345", "5", Recursion, Easy, 25, 8,
                [("12345", "5"), ("7", "1")],
                [("100", "3"), ("999999", "6")]),
            Build("Qüvvət", "Power",
                "base ədədinin exponent qüvvətini hesabla.", "Compute base raised to the power of exponent.",
                oneLineIntsFormatAz + " (base exponent)", oneLineIntsFormatEn + " (base exponent)", "Nəticə", "The result",
                "2 5", "32", Recursion, Medium, 35, 12,
                [("2 5", "32"), ("3 0", "1")],
                [("5 3", "125"), ("1 10", "1")]),

            // Exception Handling
            Build("Təhlükəsiz Bölmə", "Safe Division",
                "a-nı b-yə böl; b sıfırdırsa \"Error\" çap et.", "Divide a by b; print \"Error\" if b is zero.",
                oneLineIntsFormatAz + " (a b)", oneLineIntsFormatEn + " (a b)", "Bölmənin nəticəsi və ya Error", "The division result, or Error",
                "10 2", "5", ExceptionHandling, Easy, 25, 8,
                [("10 2", "5"), ("7 0", "Error")],
                [("9 3", "3"), ("0 5", "0")]),
            Build("Massiv İndeksi Yoxlaması", "Safe Array Index",
                "i indeksindəki elementi çap et; indeks massivdən kənardırsa \"Error\" çap et.", "Print the element at index i; print \"Error\" if the index is out of range.",
                "n, sonra n ədəd, sonra indeks i", "n, then n integers, then index i", "Element və ya Error", "The element, or Error",
                "3\n10 20 30\n1", "20", ExceptionHandling, Easy, 25, 8,
                [("3\n10 20 30\n1", "20"), ("3\n10 20 30\n5", "Error")],
                [("1\n5\n0", "5"), ("2\n1 2\n-1", "Error")]),

            // Generics
            Build("İki Dəyəri Dəyiş", "Swap Two Values",
                "İki ədədi yerini dəyişib çap et.", "Swap two numbers and print them.",
                oneLineIntsFormatAz + " (a b)", oneLineIntsFormatEn + " (a b)", "b a (yerdəyişmiş)", "b a (swapped)",
                "3 7", "7 3", Generics, Easy, 25, 8,
                [("3 7", "7 3"), ("10 20", "20 10")],
                [("0 0", "0 0"), ("-1 5", "5 -1")]),
            Build("Siyahının Son Elementi", "Last Element of List",
                "Massivin son elementini çap et.", "Print the last element of the array.",
                arrayInputFormatAz, arrayInputFormatEn, "Son element", "The last element",
                "4\n1 2 3 4", "4", Generics, Easy, 25, 8,
                [("4\n1 2 3 4", "4"), ("1\n9", "9")],
                [("3\n7 8 9", "9"), ("5\n5 4 3 2 1", "1")]),

            // Delegates & Events
            Build("Ədədləri Cütlə", "Double Each Number",
                "Massivdəki hər ədədi 2 dəfə artırıb çap et.", "Print each number in the array doubled.",
                arrayInputFormatAz, arrayInputFormatEn, "İkiqat massiv (boşluqla ayrılmış)", "The doubled array (space-separated)",
                "3\n1 2 3", "2 4 6", DelegatesEvents, Easy, 25, 8,
                [("3\n1 2 3", "2 4 6"), ("2\n5 10", "10 20")],
                [("1\n0", "0"), ("4\n-1 -2 3 4", "-2 -4 6 8")]),
            Build("Şərtli Filtr: Müsbət Ədədlər", "Filter Positive Numbers",
                "Massivdəki yalnız müsbət ədədləri çap et; heç biri yoxdursa \"NONE\" çap et.", "Print only the positive numbers in the array; print \"NONE\" if there are none.",
                arrayInputFormatAz, arrayInputFormatEn, "Müsbət ədədlər və ya NONE", "The positive numbers, or NONE",
                "5\n-1 2 -3 4 5", "2 4 5", DelegatesEvents, Medium, 35, 12,
                [("5\n-1 2 -3 4 5", "2 4 5"), ("3\n-1 -2 -3", "NONE")],
                [("1\n7", "7"), ("4\n0 -1 1 -2", "1")]),

            // Design Patterns
            Build("Ən Böyük Ortaq Bölən", "Greatest Common Divisor",
                "İki ədədin ən böyük ortaq bölənini (GCD) tap.", "Find the greatest common divisor (GCD) of two numbers.",
                oneLineIntsFormatAz + " (a b)", oneLineIntsFormatEn + " (a b)", "GCD", "The GCD",
                "12 18", "6", DesignPatterns, Medium, 35, 12,
                [("12 18", "6"), ("7 13", "1")],
                [("100 75", "25"), ("5 5", "5")]),
            Build("Massiv Elementlərinin Hasili", "Product of Array Elements",
                "Massivdəki bütün elementlərin hasilini (vurma nəticəsini) tap.", "Find the product of all elements in the array.",
                arrayInputFormatAz, arrayInputFormatEn, "Hasil", "The product",
                "4\n1 2 3 4", "24", DesignPatterns, Easy, 25, 8,
                [("4\n1 2 3 4", "24"), ("3\n2 2 2", "8")],
                [("1\n5", "5"), ("2\n0 10", "0")])
        );
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
