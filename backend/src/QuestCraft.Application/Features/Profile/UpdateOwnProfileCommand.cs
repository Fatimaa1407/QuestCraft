using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Profile;

public record UpdateOwnProfileCommand(string? Bio, string? AvatarUrl) : ICommand<MyProfileDto>;

public class UpdateOwnProfileCommandValidator : AbstractValidator<UpdateOwnProfileCommand>
{
    public UpdateOwnProfileCommandValidator()
    {
        RuleFor(x => x.Bio).MaximumLength(200).WithMessage("Bio 200 simvoldan uzun ola bilməz.");
    }
}

public class UpdateOwnProfileCommandHandler : IRequestHandler<UpdateOwnProfileCommand, MyProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateOwnProfileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MyProfileDto> Handle(UpdateOwnProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        profile.Bio = request.Bio;
        profile.AvatarUrl = request.AvatarUrl;

        await _context.SaveChangesAsync(cancellationToken);

        return new MyProfileDto(profile.Bio, profile.AvatarUrl);
    }
}
