using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Quizzes;

public record SubmitQuizAttemptCommand(int QuizId, List<QuizAnswerInput> Answers) : ICommand<QuizAttemptResultDto>;

public class SubmitQuizAttemptCommandValidator : AbstractValidator<SubmitQuizAttemptCommand>
{
    public SubmitQuizAttemptCommandValidator()
    {
        RuleFor(x => x.QuizId).GreaterThan(0);
        RuleFor(x => x.Answers).NotEmpty().WithMessage("Ən azı bir cavab göndərilməlidir.");
    }
}

public class SubmitQuizAttemptCommandHandler : IRequestHandler<SubmitQuizAttemptCommand, QuizAttemptResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDailyQuestService _dailyQuestService;
    private readonly IAchievementEvaluator _achievementEvaluator;
    private readonly IContentCompletionService _completionService;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public SubmitQuizAttemptCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDailyQuestService dailyQuestService,
        IAchievementEvaluator achievementEvaluator,
        IContentCompletionService completionService,
        IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _dailyQuestService = dailyQuestService;
        _achievementEvaluator = achievementEvaluator;
        _completionService = completionService;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<QuizAttemptResultDto> Handle(SubmitQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var quiz = await _context.Quizzes
            .Include(q => q.Questions).ThenInclude(qu => qu.Options)
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.QuizId);

        var isAdmin = _currentUser.Role == "Admin";

        if (!quiz.IsPublished && !isAdmin)
        {
            throw new NotFoundException(nameof(Quiz), request.QuizId);
        }

        if (!isAdmin && quiz.RequiredLevel > 1)
        {
            var lockUserLevel = await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .Select(p => p.Level)
                .FirstOrDefaultAsync(cancellationToken);

            if (lockUserLevel < quiz.RequiredLevel)
            {
                throw new ForbiddenException($"Bu quiz üçün Level {quiz.RequiredLevel} lazımdır.");
            }
        }

        var isEnglish = _currentUser.IsEnglish;
        var answersByQuestion = request.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionId);

        var attempt = new QuizAttempt
        {
            UserId = userId,
            QuizId = quiz.Id,
            TotalQuestions = quiz.Questions.Count,
            CompletedAt = DateTime.UtcNow,
        };

        var questionResults = new List<QuestionResultDto>();
        var score = 0;

        foreach (var question in quiz.Questions)
        {
            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
            answersByQuestion.TryGetValue(question.Id, out var selectedOptionId);
            var isCorrect = correctOption is not null && selectedOptionId == correctOption.Id;

            if (isCorrect)
            {
                score++;
            }

            attempt.Answers.Add(new QuizAttemptAnswer
            {
                QuestionId = question.Id,
                SelectedOptionId = selectedOptionId,
                IsCorrect = isCorrect,
            });

            questionResults.Add(new QuestionResultDto(
                question.Id,
                LocalizationHelper.Pick(question.Text, question.TextEn, isEnglish),
                isCorrect, selectedOptionId, correctOption?.Id ?? 0,
                LocalizationHelper.PickNullable(question.Explanation, question.ExplanationEn, isEnglish)));
        }

        var isFirstAttempt = !await _context.QuizAttempts.AnyAsync(a => a.UserId == userId && a.QuizId == quiz.Id, cancellationToken);
        var xpEarned = isFirstAttempt && quiz.Questions.Count > 0
            ? (int)Math.Round(quiz.XpReward * (double)score / quiz.Questions.Count)
            : 0;

        attempt.Score = score;
        attempt.XpEarned = xpEarned;
        _context.QuizAttempts.Add(attempt);

        var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (stats is not null && isFirstAttempt)
        {
            stats.TotalQuizzesCompleted++;
        }

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        var previousLevel = profile?.Level ?? 1;

        if (xpEarned > 0)
        {
            if (profile is not null)
            {
                profile.Xp += xpEarned;
            }

            _context.XpTransactions.Add(new XpTransaction { UserId = userId, Amount = xpEarned, Source = "Quiz" });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (streak is not null)
        {
            var hasStreakFreeze = await _context.Purchases
                .AnyAsync(p => p.UserId == userId && p.MarketplaceItem.ItemType.Name == "StreakFreeze", cancellationToken);
            StreakCalculator.RecordActivity(streak, today, hasStreakFreeze);
        }

        var activityLog = await _context.ActivityLogs.FirstOrDefaultAsync(a => a.UserId == userId && a.ActivityDate == today, cancellationToken);
        if (activityLog is null)
        {
            _context.ActivityLogs.Add(new ActivityLog { UserId = userId, ActivityDate = today, ActionCount = 1 });
        }
        else
        {
            activityLog.ActionCount++;
        }

        if (isFirstAttempt)
        {
            await _dailyQuestService.UpdateProgressAsync(userId, DailyQuestTargetType.CompleteQuiz, 1, cancellationToken);
        }
        if (xpEarned > 0)
        {
            await _dailyQuestService.UpdateProgressAsync(userId, DailyQuestTargetType.EarnXp, xpEarned, cancellationToken);
        }

        if (isFirstAttempt && profile is not null)
        {
            // Flush first so the completion count below sees this attempt.
            await _context.SaveChangesAsync(cancellationToken);
            profile.Level = await _completionService.CalculateUnlockedLevelAsync(userId, cancellationToken);
        }

        var newChallengesUnlocked = 0;
        var newQuizzesUnlocked = 0;
        if (profile is not null && profile.Level > previousLevel)
        {
            var newLevelContent = await _completionService.GetLevelCompletionAsync(userId, profile.Level, cancellationToken);
            newChallengesUnlocked = newLevelContent.ChallengesTotal;
            newQuizzesUnlocked = newLevelContent.QuizzesTotal;

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = NotificationType.LevelUp,
                Title = "Yeni səviyyə!",
                Message = $"Səviyyə {profile.Level}-ə çatdınız!",
                TitleEn = "New level!",
                MessageEn = $"You reached Level {profile.Level}!",
            });
        }

        var newAchievements = await _achievementEvaluator.EvaluateAsync(userId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        if ((profile is not null && profile.Level > previousLevel) || newAchievements.Count > 0)
        {
            await _realtimeNotifier.NotifyNewNotification(userId, cancellationToken);
        }

        return new QuizAttemptResultDto(
            attempt.Id, score, quiz.Questions.Count, xpEarned, questionResults, newAchievements.Select(a => a.Name).ToList(),
            profile?.Xp ?? 0, profile?.Coins ?? 0, profile?.Level ?? 1,
            previousLevel, newChallengesUnlocked, newQuizzesUnlocked);
    }
}
