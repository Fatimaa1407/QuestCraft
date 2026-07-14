using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record ClaimDailyQuestRewardCommand(int UserDailyQuestId) : ICommand<ClaimDailyQuestResultDto>;

public class ClaimDailyQuestRewardCommandHandler : IRequestHandler<ClaimDailyQuestRewardCommand, ClaimDailyQuestResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAchievementEvaluator _achievementEvaluator;

    public ClaimDailyQuestRewardCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAchievementEvaluator achievementEvaluator)
    {
        _context = context;
        _currentUser = currentUser;
        _achievementEvaluator = achievementEvaluator;
    }

    public async Task<ClaimDailyQuestResultDto> Handle(ClaimDailyQuestRewardCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var quest = await _context.UserDailyQuests
            .Include(q => q.DailyQuestTemplate)
            .FirstOrDefaultAsync(q => q.Id == request.UserDailyQuestId && q.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserDailyQuest), request.UserDailyQuestId);

        if (!quest.IsCompleted)
        {
            throw new ConflictException("Bu tapşırıq hələ tamamlanmayıb.");
        }

        if (quest.RewardClaimed)
        {
            throw new ConflictException("Mükafat artıq alınıb.");
        }

        quest.RewardClaimed = true;

        var isEnglish = _currentUser.IsEnglish;
        var localizedQuestTitle = LocalizationHelper.Pick(quest.DailyQuestTemplate.Title, quest.DailyQuestTemplate.TitleEn, isEnglish);

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is not null)
        {
            profile.Xp += quest.DailyQuestTemplate.XpReward;
            profile.Coins += quest.DailyQuestTemplate.CoinReward;
            // Daily quests grant XP/Coins only — Level is completion-based (see IContentCompletionService)
            // and only changes when a challenge/quiz from the current level is actually finished.
        }

        if (quest.DailyQuestTemplate.XpReward > 0)
        {
            _context.XpTransactions.Add(new XpTransaction { UserId = userId, Amount = quest.DailyQuestTemplate.XpReward, Source = "DailyQuest" });
        }

        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = NotificationType.DailyQuestReminder,
            Title = isEnglish ? "Quest reward" : "Tapşırıq mükafatı",
            Message = isEnglish
                ? $"You claimed the reward for \"{localizedQuestTitle}\"."
                : $"\"{localizedQuestTitle}\" tapşırığının mükafatını aldınız.",
        });

        var newAchievements = await _achievementEvaluator.EvaluateAsync(userId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var questDto = new DailyQuestDto(
            quest.Id,
            localizedQuestTitle,
            LocalizationHelper.PickNullable(quest.DailyQuestTemplate.Description, quest.DailyQuestTemplate.DescriptionEn, isEnglish),
            quest.CurrentProgress, quest.TargetValue, quest.IsCompleted, quest.RewardClaimed,
            quest.DailyQuestTemplate.XpReward, quest.DailyQuestTemplate.CoinReward);

        return new ClaimDailyQuestResultDto(
            questDto, profile?.Xp ?? 0, profile?.Coins ?? 0, profile?.Level ?? 0,
            newAchievements.Select(a => LocalizationHelper.Pick(a.Name, a.NameEn, isEnglish)).ToList());
    }
}
