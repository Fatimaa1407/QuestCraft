using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Users;

public record UpdateUserRoleCommand(int UserId, string Role) : ICommand<AdminUserListItemDto>;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.Role).NotEmpty().WithMessage("Rol boş ola bilməz.");
    }
}

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, AdminUserListItemDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserRoleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AdminUserListItemDto> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == _currentUser.UserId)
        {
            throw new ConflictException("Öz rolunuzu bu paneldən dəyişə bilməzsiniz.");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), request.Role);

        user.RoleId = role.Id;
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserListItemDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            role.Name,
            user.Profile?.Level ?? 1,
            user.Profile?.Xp ?? 0,
            user.Profile?.Coins ?? 0,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt);
    }
}
