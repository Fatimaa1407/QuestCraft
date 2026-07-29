using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

// Shared "battle just ended" logic used both when a participant's submission wins it outright
// (SubmitBattleSolutionCommand) and when a background sweep force-ends a stale battle
// (AutoResolveStaleBattlesCommand) — keeping ranking and reward rules in exactly one place.
public static class BattleFinalizer
{
    // Above this Jaccard ratio, two participants' solutions are flagged for admin review as likely
    // copy-pasted rather than independently written — see CodeSimilarity's doc comment for caveats.
    private const double SimilarityFlagThreshold = 0.85;

    // Compares every pair of participants who submitted anything (not just finishers — the common
    // cheating shape in a "race to solve" format is a loser's failing/partial attempt copy-pasted
    // from, or into, the eventual winner) and logs an AuditLog entry for any suspiciously close pair.
    public static void FlagSuspiciousSimilarity(IApplicationDbContext context, Battle battle)
    {
        var submitted = battle.Participants.Where(p => !string.IsNullOrEmpty(p.SubmittedCode)).ToList();
        for (var i = 0; i < submitted.Count; i++)
        {
            for (var j = i + 1; j < submitted.Count; j++)
            {
                var ratio = CodeSimilarity.ComputeRatio(submitted[i].SubmittedCode!, submitted[j].SubmittedCode!);
                if (ratio < SimilarityFlagThreshold)
                {
                    continue;
                }

                context.AuditLogs.Add(new AuditLog
                {
                    Action = "BattleSimilarityFlagged",
                    EntityName = nameof(Battle),
                    EntityId = battle.Id,
                    NewValues = $"{{\"userIdA\":{submitted[i].UserId},\"userIdB\":{submitted[j].UserId},\"similarity\":{ratio:F2}}}",
                });
            }
        }
    }

    // Assigns ranks to every participant that doesn't have one yet, best-effort-ordered by however
    // many test cases they'd passed. startingRank is 1 unless a real finisher already claimed 1st.
    public static void RankRemaining(Battle battle, int startingRank)
    {
        var remaining = battle.Participants
            .Where(p => p.Rank is null)
            .OrderByDescending(p => p.PassedTestCases)
            .ThenBy(p => p.Id)
            .ToList();

        var nextRank = startingRank;
        foreach (var p in remaining)
        {
            p.Rank = nextRank++;
        }
    }

    // Only the participant who actually finished (fully passed) and holds Rank 1 gets rewarded —
    // matches SubmitChallengeCommand's rule of "only an Accepted solve earns XP/coins", so a
    // timeout that force-ranks a partial, unfinished attempt as "1st" never pays out for it.
    // Returns the winner's UserId if a reward was granted, so the caller can evaluate achievements.
    public static int? GrantWinnerReward(IApplicationDbContext context, Battle battle)
    {
        var winner = battle.Participants.FirstOrDefault(p => p.Rank == 1 && p.HasFinished);
        if (winner is null)
        {
            return null;
        }

        var xpEarned = battle.Challenge.XpReward;
        var coinEarned = battle.Challenge.CoinReward;

        if (winner.User.Profile is not null)
        {
            winner.User.Profile.Xp += xpEarned;
            winner.User.Profile.Coins += coinEarned;
        }

        if (winner.User.Statistics is not null)
        {
            winner.User.Statistics.TotalCoinsEarned += coinEarned;
        }

        if (xpEarned > 0)
        {
            context.XpTransactions.Add(new XpTransaction { UserId = winner.UserId, Amount = xpEarned, Source = "Battle" });
        }

        context.Notifications.Add(new Notification
        {
            UserId = winner.UserId,
            Type = NotificationType.SystemNotification,
            Title = "Döyüşü qazandınız!",
            Message = $"\"{battle.Challenge.Title}\" döyüşündə 1-ci oldunuz: {xpEarned} XP və {coinEarned} coin qazandınız.",
            TitleEn = "You won the battle!",
            MessageEn = $"You finished 1st in the \"{battle.Challenge.Title}\" battle: +{xpEarned} XP and +{coinEarned} coins.",
        });

        return winner.UserId;
    }
}
