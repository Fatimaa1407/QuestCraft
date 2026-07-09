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

    public GetQuizzesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<QuizListItemDto>> Handle(GetQuizzesQuery request, CancellationToken cancellationToken)
    {
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
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuizListItemDto(q.Id, q.Title, q.Category != null ? q.Category.Name : null, q.XpReward, q.IsPublished, q.Questions.Count));

        return PagedResult<QuizListItemDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
