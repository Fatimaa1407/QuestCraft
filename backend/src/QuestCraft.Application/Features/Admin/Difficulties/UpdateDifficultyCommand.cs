using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Difficulties;

public record UpdateDifficultyCommand(int Id, string Name, string? Color, double XpMultiplier) : ICommand<DifficultyDto>;

public class UpdateDifficultyCommandValidator : AbstractValidator<UpdateDifficultyCommand>
{
    public UpdateDifficultyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(30).WithMessage("Ad 30 simvoldan uzun ola bilməz.");
        RuleFor(x => x.XpMultiplier).GreaterThan(0).WithMessage("XP əmsalı 0-dan böyük olmalıdır.");
    }
}

public class UpdateDifficultyCommandHandler : IRequestHandler<UpdateDifficultyCommand, DifficultyDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDifficultyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DifficultyDto> Handle(UpdateDifficultyCommand request, CancellationToken cancellationToken)
    {
        var difficulty = await _context.ChallengeDifficulties.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeDifficulty), request.Id);

        if (await _context.ChallengeDifficulties.AnyAsync(d => d.Name == request.Name && d.Id != request.Id, cancellationToken))
        {
            throw new ConflictException($"\"{request.Name}\" adlı çətinlik dərəcəsi artıq mövcuddur.");
        }

        difficulty.Name = request.Name;
        difficulty.Color = request.Color;
        difficulty.XpMultiplier = request.XpMultiplier;

        await _context.SaveChangesAsync(cancellationToken);

        return new DifficultyDto(difficulty.Id, difficulty.Name, difficulty.Color, difficulty.XpMultiplier);
    }
}
