using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Difficulties;

public record CreateDifficultyCommand(string Name, string? Color, double XpMultiplier) : ICommand<DifficultyDto>;

public class CreateDifficultyCommandValidator : AbstractValidator<CreateDifficultyCommand>
{
    public CreateDifficultyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(30).WithMessage("Ad 30 simvoldan uzun ola bilməz.");
        RuleFor(x => x.XpMultiplier).GreaterThan(0).WithMessage("XP əmsalı 0-dan böyük olmalıdır.");
    }
}

public class CreateDifficultyCommandHandler : IRequestHandler<CreateDifficultyCommand, DifficultyDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDifficultyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DifficultyDto> Handle(CreateDifficultyCommand request, CancellationToken cancellationToken)
    {
        if (await _context.ChallengeDifficulties.AnyAsync(d => d.Name == request.Name, cancellationToken))
        {
            throw new ConflictException($"\"{request.Name}\" adlı çətinlik dərəcəsi artıq mövcuddur.");
        }

        var difficulty = new ChallengeDifficulty
        {
            Name = request.Name,
            Color = request.Color,
            XpMultiplier = request.XpMultiplier,
        };

        _context.ChallengeDifficulties.Add(difficulty);
        await _context.SaveChangesAsync(cancellationToken);

        return new DifficultyDto(difficulty.Id, difficulty.Name, difficulty.Color, difficulty.XpMultiplier);
    }
}
