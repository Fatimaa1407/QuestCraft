using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Quizzes;

public record GetDeletedQuizzesQuery : IQuery<List<QuizListItemDto>>;

public class GetDeletedQuizzesQueryHandler : IRequestHandler<GetDeletedQuizzesQuery, List<QuizListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeletedQuizzesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<QuizListItemDto>> Handle(GetDeletedQuizzesQuery request, CancellationToken cancellationToken) =>
        _context.Quizzes
            .IgnoreQueryFilters()
            .Where(q => q.IsDeleted)
            .Include(q => q.Category)
            .Include(q => q.Questions)
            .OrderByDescending(q => q.UpdatedAt)
            .Select(q => new QuizListItemDto(
                q.Id, q.Title, q.Category != null ? q.Category.Name : null, q.XpReward, q.IsPublished,
                q.Questions.Count, q.RequiredLevel, false))
            .ToListAsync(cancellationToken);
}
