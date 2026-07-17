using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Quizzes;

public record GetQuizAttemptByIdQuery(int Id) : IQuery<QuizAttemptResultDto>;

public class GetQuizAttemptByIdQueryHandler : IRequestHandler<GetQuizAttemptByIdQuery, QuizAttemptResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuizAttemptByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuizAttemptResultDto> Handle(GetQuizAttemptByIdQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _context.QuizAttempts
            .Include(a => a.Answers).ThenInclude(ans => ans.Question).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(QuizAttempt), request.Id);

        var isOwner = attempt.UserId == _currentUser.UserId;
        var isAdmin = _currentUser.Role == "Admin";
        if (!isOwner && !isAdmin)
        {
            throw new NotFoundException(nameof(QuizAttempt), request.Id);
        }

        var isEnglish = _currentUser.IsEnglish;
        var questionResults = attempt.Answers.Select(a => new QuestionResultDto(
            a.QuestionId,
            LocalizationHelper.Pick(a.Question.Text, a.Question.TextEn, isEnglish),
            a.IsCorrect,
            a.SelectedOptionId,
            a.Question.Options.FirstOrDefault(o => o.IsCorrect)?.Id ?? 0,
            LocalizationHelper.PickNullable(a.Question.Explanation, a.Question.ExplanationEn, isEnglish))).ToList();

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == attempt.UserId, cancellationToken);

        return new QuizAttemptResultDto(
            attempt.Id, attempt.Score, attempt.TotalQuestions, attempt.XpEarned, questionResults, [],
            profile?.Xp ?? 0, profile?.Coins ?? 0, profile?.Level ?? 1,
            // Historical view of a past attempt — never re-trigger the level-up celebration.
            profile?.Level ?? 1, 0, 0);
    }
}
