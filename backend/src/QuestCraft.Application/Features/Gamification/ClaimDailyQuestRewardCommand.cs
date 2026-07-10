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

    public ClaimDailyQuestRewardCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
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

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is not null)
        {
            profile.Xp += quest.DailyQuestTemplate.XpReward;
            profile.Coins += quest.DailyQuestTemplate.CoinReward;
            profile.Level = GamificationCalculator.CalculateLevel(profile.Xp);
        }

        if (quest.DailyQuestTemplate.XpReward > 0)
        {
            _context.XpTransactions.Add(new XpTransaction { UserId = userId, Amount = quest.DailyQuestTemplate.XpReward, Source = "DailyQuest" });
        }

        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = NotificationType.DailyQuestReminder,
            Title = "Tapşırıq mükafatı",
            Message = $"\"{quest.DailyQuestTemplate.Title}\" tapşırığının mükafatını aldınız.",
        });

        await _context.SaveChangesAsync(cancellationToken);

        var questDto = new DailyQuestDto(
            quest.Id, quest.DailyQuestTemplate.Title, quest.DailyQuestTemplate.Description,
            quest.CurrentProgress, quest.TargetValue, quest.IsCompleted, quest.RewardClaimed,
            quest.DailyQuestTemplate.XpReward, quest.DailyQuestTemplate.CoinReward);

        return new ClaimDailyQuestResultDto(questDto, profile?.Xp ?? 0, profile?.Coins ?? 0, profile?.Level ?? 0);
    }
}
