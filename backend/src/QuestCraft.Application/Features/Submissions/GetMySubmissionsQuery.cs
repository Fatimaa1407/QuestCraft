using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Common.Models;

namespace QuestCraft.Application.Features.Submissions;

public record GetMySubmissionsQuery(int Page, int PageSize) : IQuery<PagedResult<SubmissionListItemDto>>;

public class GetMySubmissionsQueryHandler : IRequestHandler<GetMySubmissionsQuery, PagedResult<SubmissionListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMySubmissionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<PagedResult<SubmissionListItemDto>> Handle(GetMySubmissionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var query = _context.ChallengeSubmissions
            .Include(s => s.Challenge)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new SubmissionListItemDto(s.Id, s.ChallengeId, s.Challenge.Title, s.Verdict.ToString(), s.SubmittedAt, s.ExecutionTimeMs));

        return PagedResult<SubmissionListItemDto>.CreateAsync(query, request.Page, request.PageSize, cancellationToken);
    }
}
