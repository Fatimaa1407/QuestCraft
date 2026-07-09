using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Difficulties;

public record RestoreDifficultyCommand(int Id) : ICommand<DifficultyDto>;

public class RestoreDifficultyCommandHandler : IRequestHandler<RestoreDifficultyCommand, DifficultyDto>
{
    private readonly IApplicationDbContext _context;

    public RestoreDifficultyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DifficultyDto> Handle(RestoreDifficultyCommand request, CancellationToken cancellationToken)
    {
        var difficulty = await _context.ChallengeDifficulties
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeDifficulty), request.Id);

        if (await _context.ChallengeDifficulties.AnyAsync(d => d.Name == difficulty.Name && d.Id != difficulty.Id, cancellationToken))
        {
            throw new ConflictException($"\"{difficulty.Name}\" adlı aktiv çətinlik dərəcəsi artıq mövcuddur, əvvəlcə onu dəyişin.");
        }

        difficulty.IsDeleted = false;
        await _context.SaveChangesAsync(cancellationToken);

        return new DifficultyDto(difficulty.Id, difficulty.Name, difficulty.Color, difficulty.XpMultiplier);
    }
}
