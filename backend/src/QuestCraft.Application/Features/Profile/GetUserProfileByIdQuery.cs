using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Profile;

public record GetUserProfileByIdQuery(int UserId) : IQuery<PublicProfileDto>;

public class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, PublicProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserProfileByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PublicProfileDto> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var isEnglish = _currentUser.IsEnglish;

        var profile = await _context.UserProfiles
            .Include(p => p.User)
            .Include(p => p.EquippedAvatar)
            .Include(p => p.EquippedFrame)
            .Include(p => p.EquippedBanner)
            .Include(p => p.EquippedTitle)
            .Include(p => p.EquippedBadge)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), request.UserId);

        return new PublicProfileDto(
            UserId: profile.UserId,
            Username: profile.User.Username,
            Level: profile.Level,
            Xp: profile.Xp,
            Bio: profile.Bio,
            AvatarUrl: profile.EquippedAvatar?.ImageUrl ?? profile.AvatarUrl,
            FrameImageUrl: profile.EquippedFrame?.ImageUrl,
            BannerImageUrl: profile.EquippedBanner?.ImageUrl,
            TitleText: profile.EquippedTitle is null ? null : LocalizationHelper.Pick(profile.EquippedTitle.Name, profile.EquippedTitle.NameEn, isEnglish),
            BadgeImageUrl: profile.EquippedBadge?.ImageUrl,
            BadgeName: profile.EquippedBadge is null ? null : LocalizationHelper.Pick(profile.EquippedBadge.Name, profile.EquippedBadge.NameEn, isEnglish),
            JoinedAt: profile.User.CreatedAt,
            IsGameComplete: profile.GameCompletedAt is not null);
    }
}
