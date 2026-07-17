using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

// UserId null = run for every active user (the normal weekly-tick path). UserId set + Force = true
// bypasses the "already sent this week" dedupe check — used by the admin on-demand trigger for
// testing, since waiting for a real 7-day window isn't practical to verify by hand.
public record SendWeeklyRecapCommand(int? UserId = null, bool Force = false) : ICommand<int>;

public class SendWeeklyRecapCommandHandler : IRequestHandler<SendWeeklyRecapCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public SendWeeklyRecapCommandHandler(IApplicationDbContext context, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<int> Handle(SendWeeklyRecapCommand request, CancellationToken cancellationToken)
    {
        var windowStart = DateTime.UtcNow.AddDays(-7);
        var dedupeSince = DateTime.UtcNow.AddDays(-6);

        var userIds = request.UserId is not null
            ? [request.UserId.Value]
            : await _context.Users.Where(u => u.IsActive).Select(u => u.Id).ToListAsync(cancellationToken);

        var notifiedUserIds = new List<int>();

        foreach (var userId in userIds)
        {
            if (!request.Force)
            {
                var alreadySent = await _context.Notifications
                    .AnyAsync(n => n.UserId == userId && n.Type == NotificationType.WeeklyRecap && n.CreatedAt >= dedupeSince, cancellationToken);
                if (alreadySent)
                {
                    continue;
                }
            }

            var challengesSolved = await _context.ChallengeSubmissions
                .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted && s.SubmittedAt >= windowStart)
                .Select(s => s.ChallengeId)
                .Distinct()
                .CountAsync(cancellationToken);

            var quizzesCompleted = await _context.QuizAttempts
                .CountAsync(a => a.UserId == userId && a.CompletedAt >= windowStart, cancellationToken);

            var xpEarned = await _context.XpTransactions
                .Where(x => x.UserId == userId && x.EarnedAt >= windowStart)
                .SumAsync(x => (int?)x.Amount, cancellationToken) ?? 0;

            if (challengesSolved == 0 && quizzesCompleted == 0 && xpEarned == 0)
            {
                continue;
            }

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = NotificationType.WeeklyRecap,
                Title = "Həftəlik icmalınız",
                Message = $"Bu həftə {challengesSolved} challenge həll etdiniz, {quizzesCompleted} quiz tamamladınız və {xpEarned} XP qazandınız!",
                TitleEn = "Your weekly recap",
                MessageEn = $"This week you solved {challengesSolved} challenges, completed {quizzesCompleted} quizzes, and earned {xpEarned} XP!",
            });

            notifiedUserIds.Add(userId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var userId in notifiedUserIds)
        {
            await _realtimeNotifier.NotifyNewNotification(userId, cancellationToken);
        }

        return notifiedUserIds.Count;
    }
}
