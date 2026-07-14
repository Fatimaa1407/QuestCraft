namespace QuestCraft.Application.Features.Quizzes;

public record QuizAnswerInput(int QuestionId, int? SelectedOptionId);

public record QuestionResultDto(int QuestionId, string Text, bool IsCorrect, int? SelectedOptionId, int CorrectOptionId, string? Explanation);

public record QuizAttemptResultDto(
    int AttemptId,
    int Score,
    int TotalQuestions,
    int XpEarned,
    List<QuestionResultDto> Questions,
    List<string> NewAchievements,
    int TotalXp,
    int TotalCoins,
    int Level);

public record QuizAttemptListItemDto(int Id, int QuizId, string QuizTitle, int Score, int TotalQuestions, int XpEarned, DateTime CompletedAt);
