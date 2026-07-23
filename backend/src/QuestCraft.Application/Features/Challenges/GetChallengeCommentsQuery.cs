using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Challenges;

public record ChallengeCommentThreadDto(ChallengeCommentDto Comment, List<ChallengeCommentDto> Replies);

public record GetChallengeCommentsQuery(int ChallengeId) : IQuery<List<ChallengeCommentThreadDto>>;

public class GetChallengeCommentsQueryHandler : IRequestHandler<GetChallengeCommentsQuery, List<ChallengeCommentThreadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetChallengeCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChallengeCommentThreadDto>> Handle(GetChallengeCommentsQuery request, CancellationToken cancellationToken)
    {
        var all = await _context.ChallengeComments
            .Where(c => c.ChallengeId == request.ChallengeId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new ChallengeCommentDto(
                c.Id, c.Content, c.IsSpoiler, c.CreatedAt,
                c.UserId, c.User.Username, c.User.Profile!.AvatarUrl, c.ParentCommentId))
            .ToListAsync(cancellationToken);

        var repliesByParent = all.Where(c => c.ParentCommentId is not null)
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        return all.Where(c => c.ParentCommentId is null)
            .Select(c => new ChallengeCommentThreadDto(c, repliesByParent.GetValueOrDefault(c.Id, [])))
            .OrderByDescending(t => t.Comment.CreatedAt)
            .ToList();
    }
}
