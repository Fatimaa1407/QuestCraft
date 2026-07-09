using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class UserStatistics : BaseEntity
{
    public int TotalSubmissions { get; set; }
    public int AcceptedSubmissions { get; set; }
    public int TotalChallengesSolved { get; set; }
    public int TotalQuizzesCompleted { get; set; }
    public int TotalCoinsEarned { get; set; }
    public int TotalCoinsSpent { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
