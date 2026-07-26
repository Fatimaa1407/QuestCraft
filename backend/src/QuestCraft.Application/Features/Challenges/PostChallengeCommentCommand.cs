using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Challenges;

public record ChallengeCommentDto(
    int Id, string Content, bool IsSpoiler, DateTime CreatedAt,
    int UserId, string Username, string? AvatarUrl, int? ParentCommentId,
    string? FrameImageUrl = null, string? TitleText = null, string? BadgeImageUrl = null, string? BadgeName = null);

public record PostChallengeCommentCommand(int ChallengeId, string Content, bool IsSpoiler, int? ParentCommentId) : ICommand<ChallengeCommentDto>;

public class PostChallengeCommentCommandValidator : AbstractValidator<PostChallengeCommentCommand>
{
    public PostChallengeCommentCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().WithMessage("Şərh boş ola bilməz.")
            .MaximumLength(1000).WithMessage("Şərh 1000 simvoldan uzun ola bilməz.");
    }
}

public class PostChallengeCommentCommandHandler : IRequestHandler<PostChallengeCommentCommand, ChallengeCommentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PostChallengeCommentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ChallengeCommentDto> Handle(PostChallengeCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var challengeExists = await _context.Challenges.AnyAsync(c => c.Id == request.ChallengeId, cancellationToken);
        if (!challengeExists)
        {
            throw new NotFoundException(nameof(Challenge), request.ChallengeId);
        }

        if (request.ParentCommentId is not null)
        {
            var parent = await _context.ChallengeComments
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId, cancellationToken)
                ?? throw new NotFoundException(nameof(ChallengeComment), request.ParentCommentId);

            if (parent.ChallengeId != request.ChallengeId)
            {
                throw new BadRequestException("Cavab eyni challenge-ə aid olmalıdır.");
            }
            if (parent.ParentCommentId is not null)
            {
                throw new BadRequestException("Cavaba cavab yazıla bilməz.");
            }
        }

        var comment = new ChallengeComment
        {
            ChallengeId = request.ChallengeId,
            UserId = userId,
            Content = request.Content.Trim(),
            IsSpoiler = request.IsSpoiler,
            ParentCommentId = request.ParentCommentId,
        };
        _context.ChallengeComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        var isEnglish = _currentUser.IsEnglish;
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Username,
                AvatarUrl = u.Profile!.EquippedAvatar != null ? u.Profile.EquippedAvatar.ImageUrl : u.Profile.AvatarUrl,
                FrameImageUrl = u.Profile.EquippedFrame != null ? u.Profile.EquippedFrame.ImageUrl : null,
                TitleName = u.Profile.EquippedTitle != null ? u.Profile.EquippedTitle.Name : null,
                TitleNameEn = u.Profile.EquippedTitle != null ? u.Profile.EquippedTitle.NameEn : null,
                BadgeImageUrl = u.Profile.EquippedBadge != null ? u.Profile.EquippedBadge.ImageUrl : null,
                BadgeName = u.Profile.EquippedBadge != null ? u.Profile.EquippedBadge.Name : null,
                BadgeNameEn = u.Profile.EquippedBadge != null ? u.Profile.EquippedBadge.NameEn : null,
            })
            .FirstAsync(cancellationToken);

        return new ChallengeCommentDto(comment.Id, comment.Content, comment.IsSpoiler, comment.CreatedAt,
            userId, user.Username, user.AvatarUrl, comment.ParentCommentId,
            user.FrameImageUrl, LocalizationHelper.PickNullable(user.TitleName, user.TitleNameEn, isEnglish),
            user.BadgeImageUrl, LocalizationHelper.PickNullable(user.BadgeName, user.BadgeNameEn, isEnglish));
    }
}
