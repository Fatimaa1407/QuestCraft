using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Common.Models;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record GetQuizzesQuery(int? CategoryId, string? Search, int Page, int PageSize, bool IncludeUnpublished)
    : IQuery<PagedResult<QuizListItemDto>>;

public class GetQuizzesQueryHandler : IRequestHandler<GetQuizzesQuery, PagedResult<QuizListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuizzesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<QuizListItemDto>> Handle(GetQuizzesQuery request, CancellationToken cancellationToken)
    {
        var isEnglish = _currentUser.IsEnglish;
        var isAdmin = _currentUser.Role == "Admin";
        var userLevel = 1;
        if (!isAdmin && _currentUser.UserId is not null)
        {
            userLevel = await _context.UserProfiles
                .Where(p => p.UserId == _currentUser.UserId)
                .Select(p => p.Level)
                .FirstOrDefaultAsync(cancellationToken);
            if (userLevel == 0)
            {
                userLevel = 1;
            }
        }

        var query = _context.Quizzes.Include(q => q.Category).Include(q => q.Questions).AsQueryable();

        if (!request.IncludeUnpublished)
        {
            query = query.Where(q => q.IsPublished);
        }

        if (request.CategoryId is not null)
        {
            query = query.Where(q => q.CategoryId == request.CategoryId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(q => q.Title.Contains(request.Search));
        }

        var projected = query
            .OrderBy(q => q.RequiredLevel)
            .ThenByDescending(q => q.CreatedAt)
            .Select(q => new QuizListItemDto(
                q.Id,
                isEnglish && q.TitleEn != null && q.TitleEn != "" ? q.TitleEn : q.Title,
                q.Category != null ? q.Category.Name : null, q.XpReward, q.IsPublished, q.Questions.Count,
                q.RequiredLevel, !isAdmin && q.RequiredLevel > userLevel));

        return await PagedResult<QuizListItemDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
