using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Gamification;

public record GetDailyQuestsQuery : IQuery<List<DailyQuestDto>>;

public class GetDailyQuestsQueryHandler : IRequestHandler<GetDailyQuestsQuery, List<DailyQuestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDailyQuestService _dailyQuestService;

    public GetDailyQuestsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IDailyQuestService dailyQuestService)
    {
        _context = context;
        _currentUser = currentUser;
        _dailyQuestService = dailyQuestService;
    }

    public async Task<List<DailyQuestDto>> Handle(GetDailyQuestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        await _dailyQuestService.EnsureTodayQuestsAsync(userId, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.UserDailyQuests
            .Include(q => q.DailyQuestTemplate)
            .Where(q => q.UserId == userId && q.QuestDate == today)
            .Select(q => new DailyQuestDto(
                q.Id, q.DailyQuestTemplate.Title, q.DailyQuestTemplate.Description,
                q.CurrentProgress, q.TargetValue, q.IsCompleted, q.RewardClaimed,
                q.DailyQuestTemplate.XpReward, q.DailyQuestTemplate.CoinReward))
            .ToListAsync(cancellationToken);
    }
}
