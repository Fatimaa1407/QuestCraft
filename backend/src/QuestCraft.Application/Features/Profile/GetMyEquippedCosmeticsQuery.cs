using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Profile;

public record GetMyEquippedCosmeticsQuery : IQuery<EquippedCosmeticsDto>;

public class GetMyEquippedCosmeticsQueryHandler : IRequestHandler<GetMyEquippedCosmeticsQuery, EquippedCosmeticsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyEquippedCosmeticsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<EquippedCosmeticsDto> Handle(GetMyEquippedCosmeticsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles
            .Include(p => p.EquippedAvatar)
            .Include(p => p.EquippedFrame)
            .Include(p => p.EquippedBanner)
            .Include(p => p.EquippedTitle)
            .Include(p => p.EquippedBadge)
            .Include(p => p.EquippedTheme)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        var isEnglish = _currentUser.IsEnglish;

        return new EquippedCosmeticsDto(
            AvatarUrl: profile.EquippedAvatar?.ImageUrl ?? profile.AvatarUrl,
            FrameImageUrl: profile.EquippedFrame?.ImageUrl,
            BannerImageUrl: profile.EquippedBanner?.ImageUrl,
            TitleText: profile.EquippedTitle is null ? null : LocalizationHelper.Pick(profile.EquippedTitle.Name, profile.EquippedTitle.NameEn, isEnglish),
            BadgeImageUrl: profile.EquippedBadge?.ImageUrl,
            BadgeName: profile.EquippedBadge is null ? null : LocalizationHelper.Pick(profile.EquippedBadge.Name, profile.EquippedBadge.NameEn, isEnglish),
            ThemeItemId: profile.EquippedThemeId,
            ThemeName: profile.EquippedTheme?.Name);
    }
}
