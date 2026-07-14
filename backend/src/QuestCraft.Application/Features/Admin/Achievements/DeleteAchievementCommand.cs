using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Achievements;

public record DeleteAchievementCommand(int Id) : ICommand<Unit>;

public class DeleteAchievementCommandHandler : IRequestHandler<DeleteAchievementCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Achievement), request.Id);

        var inUse = await _context.UserAchievements.AnyAsync(ua => ua.AchievementId == request.Id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Bu nailiyyəti artıq qazanan istifadəçilər olduğu üçün silinə bilməz. Əvəzinə deaktiv edin.");
        }

        achievement.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
