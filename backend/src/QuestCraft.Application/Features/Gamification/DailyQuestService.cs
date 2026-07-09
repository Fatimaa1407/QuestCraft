using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

/// <summary>
/// Quests for "today" are generated lazily on first access/progress-update rather than by a midnight
/// cron job — simpler and more robust for a project that isn't guaranteed to be running at 00:00.
/// UpdateProgressAsync stages changes on the context; the caller commits via SaveChangesAsync.
/// </summary>
public interface IDailyQuestService
{
    Task EnsureTodayQuestsAsync(int userId, CancellationToken cancellationToken);
    Task UpdateProgressAsync(int userId, DailyQuestTargetType targetType, int amount, CancellationToken cancellationToken);
}

public class DailyQuestService : IDailyQuestService
{
    private static readonly Random Random = new();

    private readonly IApplicationDbContext _context;

    public DailyQuestService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureTodayQuestsAsync(int userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var hasToday = await _context.UserDailyQuests
            .AnyAsync(q => q.UserId == userId && q.QuestDate == today, cancellationToken);
        if (hasToday)
        {
            return;
        }

        var templates = await _context.DailyQuestTemplates.Where(t => t.IsActive).ToListAsync(cancellationToken);
        if (templates.Count == 0)
        {
            return;
        }

        var selected = templates.OrderBy(_ => Random.Next()).Take(Math.Min(3, templates.Count));

        foreach (var template in selected)
        {
            _context.UserDailyQuests.Add(new UserDailyQuest
            {
                UserId = userId,
                DailyQuestTemplateId = template.Id,
                QuestDate = today,
                TargetValue = template.TargetValue,
                CurrentProgress = 0,
            });
        }

        // Committed immediately so the rows are visible to the query in UpdateProgressAsync right after.
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateProgressAsync(int userId, DailyQuestTargetType targetType, int amount, CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return;
        }

        await EnsureTodayQuestsAsync(userId, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var quests = await _context.UserDailyQuests
            .Include(q => q.DailyQuestTemplate)
            .Where(q => q.UserId == userId && q.QuestDate == today && !q.IsCompleted
                && q.DailyQuestTemplate.TargetType == targetType)
            .ToListAsync(cancellationToken);

        foreach (var quest in quests)
        {
            quest.CurrentProgress = Math.Min(quest.TargetValue, quest.CurrentProgress + amount);
            if (quest.CurrentProgress >= quest.TargetValue)
            {
                quest.IsCompleted = true;
            }
        }
    }
}
