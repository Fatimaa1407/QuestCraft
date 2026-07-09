using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Common.Models;

namespace QuestCraft.Application.Features.Quizzes;

public record GetMyQuizAttemptsQuery(int Page, int PageSize) : IQuery<PagedResult<QuizAttemptListItemDto>>;

public class GetMyQuizAttemptsQueryHandler : IRequestHandler<GetMyQuizAttemptsQuery, PagedResult<QuizAttemptListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyQuizAttemptsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<PagedResult<QuizAttemptListItemDto>> Handle(GetMyQuizAttemptsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var query = _context.QuizAttempts
            .Include(a => a.Quiz)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CompletedAt)
            .Select(a => new QuizAttemptListItemDto(a.Id, a.QuizId, a.Quiz.Title, a.Score, a.TotalQuestions, a.XpEarned, a.CompletedAt));

        return PagedResult<QuizAttemptListItemDto>.CreateAsync(query, request.Page, request.PageSize, cancellationToken);
    }
}
