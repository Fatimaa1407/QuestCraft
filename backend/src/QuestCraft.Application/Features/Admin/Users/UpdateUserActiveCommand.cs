using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Users;

public record UpdateUserActiveCommand(int UserId, bool IsActive) : ICommand<AdminUserListItemDto>;

public class UpdateUserActiveCommandHandler : IRequestHandler<UpdateUserActiveCommand, AdminUserListItemDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserActiveCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AdminUserListItemDto> Handle(UpdateUserActiveCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == _currentUser.UserId)
        {
            throw new ConflictException("Öz hesabınızı bu paneldən deaktiv edə bilməzsiniz.");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserListItemDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.Name,
            user.Profile?.Level ?? 1,
            user.Profile?.Xp ?? 0,
            user.Profile?.Coins ?? 0,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt);
    }
}
