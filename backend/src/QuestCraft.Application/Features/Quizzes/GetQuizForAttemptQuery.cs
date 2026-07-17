using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Admin.Quizzes;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Quizzes;

public record GetQuizForAttemptQuery(int Id) : IQuery<QuizAttemptViewDto>;

public class GetQuizForAttemptQueryHandler : IRequestHandler<GetQuizForAttemptQuery, QuizAttemptViewDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuizForAttemptQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuizAttemptViewDto> Handle(GetQuizForAttemptQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions).ThenInclude(qu => qu.Options)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.Id);

        var isAdmin = _currentUser.Role == "Admin";

        if (!quiz.IsPublished && !isAdmin)
        {
            throw new NotFoundException(nameof(Quiz), request.Id);
        }

        if (!isAdmin && quiz.RequiredLevel > 1)
        {
            var userLevel = _currentUser.UserId is null
                ? 1
                : await _context.UserProfiles
                    .Where(p => p.UserId == _currentUser.UserId)
                    .Select(p => p.Level)
                    .FirstOrDefaultAsync(cancellationToken);

            if (userLevel < quiz.RequiredLevel)
            {
                throw new ForbiddenException($"Bu quiz üçün Level {quiz.RequiredLevel} lazımdır.");
            }
        }

        var isEnglish = _currentUser.IsEnglish;
        // Options are graded by Id, not position (see SubmitQuizAttemptCommandHandler), so
        // shuffling the display order here is safe — it just stops the correct answer from
        // always landing in the same slot for questions where admins left it on option 1.
        var questions = quiz.Questions.Select(q => new QuestionDto(
            q.Id,
            LocalizationHelper.Pick(q.Text, q.TextEn, isEnglish),
            q.Options
                .OrderBy(_ => Random.Shared.Next())
                .Select(o => new QuestionOptionDto(o.Id, LocalizationHelper.Pick(o.Text, o.TextEn, isEnglish)))
                .ToList()))
            .ToList();

        var isAlreadyCompleted = _currentUser.UserId is not null &&
            await _context.QuizAttempts.AnyAsync(a => a.UserId == _currentUser.UserId && a.QuizId == request.Id, cancellationToken);

        return new QuizAttemptViewDto(quiz.Id, LocalizationHelper.Pick(quiz.Title, quiz.TitleEn, isEnglish), quiz.XpReward, questions, isAlreadyCompleted);
    }
}
