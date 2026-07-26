using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Challenges;

public record ChallengeCommentThreadDto(ChallengeCommentDto Comment, List<ChallengeCommentDto> Replies);

public record GetChallengeCommentsQuery(int ChallengeId) : IQuery<List<ChallengeCommentThreadDto>>;

public class GetChallengeCommentsQueryHandler : IRequestHandler<GetChallengeCommentsQuery, List<ChallengeCommentThreadDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChallengeCommentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ChallengeCommentThreadDto>> Handle(GetChallengeCommentsQuery request, CancellationToken cancellationToken)
    {
        var isEnglish = _currentUser.IsEnglish;
        var all = await _context.ChallengeComments
            .Where(c => c.ChallengeId == request.ChallengeId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id, c.Content, c.IsSpoiler, c.CreatedAt, c.UserId, c.User.Username,
                AvatarUrl = c.User.Profile!.EquippedAvatar != null ? c.User.Profile.EquippedAvatar.ImageUrl : c.User.Profile.AvatarUrl,
                c.ParentCommentId,
                FrameImageUrl = c.User.Profile.EquippedFrame != null ? c.User.Profile.EquippedFrame.ImageUrl : null,
                TitleName = c.User.Profile.EquippedTitle != null ? c.User.Profile.EquippedTitle.Name : null,
                TitleNameEn = c.User.Profile.EquippedTitle != null ? c.User.Profile.EquippedTitle.NameEn : null,
                BadgeImageUrl = c.User.Profile.EquippedBadge != null ? c.User.Profile.EquippedBadge.ImageUrl : null,
                BadgeName = c.User.Profile.EquippedBadge != null ? c.User.Profile.EquippedBadge.Name : null,
                BadgeNameEn = c.User.Profile.EquippedBadge != null ? c.User.Profile.EquippedBadge.NameEn : null,
            })
            .ToListAsync(cancellationToken);

        var allDtos = all.Select(c => new ChallengeCommentDto(
                c.Id, c.Content, c.IsSpoiler, c.CreatedAt, c.UserId, c.Username, c.AvatarUrl, c.ParentCommentId,
                c.FrameImageUrl, LocalizationHelper.PickNullable(c.TitleName, c.TitleNameEn, isEnglish),
                c.BadgeImageUrl, LocalizationHelper.PickNullable(c.BadgeName, c.BadgeNameEn, isEnglish)))
            .ToList();

        var repliesByParent = allDtos.Where(c => c.ParentCommentId is not null)
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        return allDtos.Where(c => c.ParentCommentId is null)
            .Select(c => new ChallengeCommentThreadDto(c, repliesByParent.GetValueOrDefault(c.Id, [])))
            .OrderByDescending(t => t.Comment.CreatedAt)
            .ToList();
    }
}
