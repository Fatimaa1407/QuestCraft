using MediatR;
using Microsoft.EntityFrameworkCore;
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

        var questionResults = attempt.Answers.Select(a => new QuestionResultDto(
            a.QuestionId,
            a.Question.Text,
            a.IsCorrect,
            a.SelectedOptionId,
            a.Question.Options.FirstOrDefault(o => o.IsCorrect)?.Id ?? 0,
            a.Question.Explanation)).ToList();

        return new QuizAttemptResultDto(attempt.Id, attempt.Score, attempt.TotalQuestions, attempt.XpEarned, questionResults, []);
    }
}
