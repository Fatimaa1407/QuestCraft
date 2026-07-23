using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Challenges;

public record ChallengeCommentDto(
    int Id, string Content, bool IsSpoiler, DateTime CreatedAt,
    int UserId, string Username, string? AvatarUrl, int? ParentCommentId);

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

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Username, u.Profile!.AvatarUrl })
            .FirstAsync(cancellationToken);

        return new ChallengeCommentDto(comment.Id, comment.Content, comment.IsSpoiler, comment.CreatedAt,
            userId, user.Username, user.AvatarUrl, comment.ParentCommentId);
    }
}
