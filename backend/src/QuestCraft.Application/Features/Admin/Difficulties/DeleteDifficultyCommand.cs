using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Difficulties;

public record DeleteDifficultyCommand(int Id) : ICommand<Unit>;

public class DeleteDifficultyCommandHandler : IRequestHandler<DeleteDifficultyCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteDifficultyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteDifficultyCommand request, CancellationToken cancellationToken)
    {
        var difficulty = await _context.ChallengeDifficulties.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeDifficulty), request.Id);

        var inUse = await _context.Challenges.AnyAsync(c => c.DifficultyId == request.Id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Bu çətinlik dərəcəsinə aid challenge-lər olduğu üçün silinə bilməz.");
        }

        difficulty.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
