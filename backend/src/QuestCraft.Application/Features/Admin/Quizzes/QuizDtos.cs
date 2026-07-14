namespace QuestCraft.Application.Features.Admin.Quizzes;

public record QuizListItemDto(int Id, string Title, string? Category, int XpReward, bool IsPublished, int QuestionCount, int RequiredLevel, bool IsLocked);

public record QuestionOptionDto(int Id, string Text);

public record QuestionOptionAdminDto(int Id, string Text, bool IsCorrect, string? TextEn);

public record QuestionDto(int Id, string Text, List<QuestionOptionDto> Options);

public record QuestionAdminDto(int Id, string Text, string? Explanation, List<QuestionOptionAdminDto> Options, string? TextEn, string? ExplanationEn);

// Safe for students taking the quiz — never exposes which option is correct.
public record QuizAttemptViewDto(int Id, string Title, int XpReward, List<QuestionDto> Questions, bool IsAlreadyCompleted);

// Full detail for admins editing the quiz — includes correct-answer flags.
public record QuizAdminDetailDto(
    int Id, string Title, int? CategoryId, string? Category, int XpReward, bool IsPublished, int RequiredLevel,
    List<QuestionAdminDto> Questions, string? TitleEn);
