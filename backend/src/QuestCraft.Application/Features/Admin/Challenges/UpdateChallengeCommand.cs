using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record UpdateChallengeCommand(
    int Id,
    string Title,
    string Description,
    int CategoryId,
    int DifficultyId,
    int TimeLimitMs,
    int MemoryLimitMb,
    int XpReward,
    int CoinReward,
    string StarterCode,
    string? Constraints,
    string? InputFormat,
    string? OutputFormat,
    string? SampleInput,
    string? SampleOutput,
    string? Hint,
    bool IsPublished,
    int RequiredLevel = 1,
    string? TitleEn = null,
    string? DescriptionEn = null,
    string? ConstraintsEn = null,
    string? InputFormatEn = null,
    string? OutputFormatEn = null,
    string? HintEn = null,
    string? StarterCodeEn = null) : ICommand<ChallengeDetailDto>;

public class UpdateChallengeCommandValidator : AbstractValidator<UpdateChallengeCommand>
{
    public UpdateChallengeCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlıq boş ola bilməz.")
            .MaximumLength(200).WithMessage("Başlıq 200 simvoldan uzun ola bilməz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Təsvir boş ola bilməz.");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Kateqoriya seçilməlidir.");
        RuleFor(x => x.DifficultyId).GreaterThan(0).WithMessage("Çətinlik dərəcəsi seçilməlidir.");
        RuleFor(x => x.TimeLimitMs).GreaterThan(0).WithMessage("Vaxt limiti müsbət olmalıdır.");
        RuleFor(x => x.MemoryLimitMb).GreaterThan(0).WithMessage("Yaddaş limiti müsbət olmalıdır.");
        RuleFor(x => x.XpReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CoinReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StarterCode).NotEmpty().WithMessage("Starter code boş ola bilməz.");
        RuleFor(x => x.RequiredLevel).GreaterThanOrEqualTo(1).WithMessage("Tələb olunan level ən azı 1 olmalıdır.");
    }
}

public class UpdateChallengeCommandHandler : IRequestHandler<UpdateChallengeCommand, ChallengeDetailDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateChallengeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChallengeDetailDto> Handle(UpdateChallengeCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges
            .Include(c => c.TestCases)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Challenge), request.Id);

        var category = await _context.ChallengeCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeCategory), request.CategoryId);

        var difficulty = await _context.ChallengeDifficulties.FirstOrDefaultAsync(d => d.Id == request.DifficultyId, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeDifficulty), request.DifficultyId);

        challenge.Title = request.Title;
        challenge.Description = request.Description;
        challenge.CategoryId = category.Id;
        challenge.DifficultyId = difficulty.Id;
        challenge.TimeLimitMs = request.TimeLimitMs;
        challenge.MemoryLimitMb = request.MemoryLimitMb;
        challenge.XpReward = request.XpReward;
        challenge.CoinReward = request.CoinReward;
        challenge.StarterCode = request.StarterCode;
        challenge.Constraints = request.Constraints;
        challenge.InputFormat = request.InputFormat;
        challenge.OutputFormat = request.OutputFormat;
        challenge.SampleInput = request.SampleInput;
        challenge.SampleOutput = request.SampleOutput;
        challenge.Hint = request.Hint;
        challenge.IsPublished = request.IsPublished;
        challenge.RequiredLevel = request.RequiredLevel;
        challenge.TitleEn = request.TitleEn;
        challenge.DescriptionEn = request.DescriptionEn;
        challenge.ConstraintsEn = request.ConstraintsEn;
        challenge.InputFormatEn = request.InputFormatEn;
        challenge.OutputFormatEn = request.OutputFormatEn;
        challenge.HintEn = request.HintEn;
        challenge.StarterCodeEn = request.StarterCodeEn;

        await _context.SaveChangesAsync(cancellationToken);

        return new ChallengeDetailDto(
            challenge.Id, challenge.Title, challenge.Description,
            challenge.CategoryId, category.Name, challenge.DifficultyId, difficulty.Name,
            challenge.TimeLimitMs, challenge.MemoryLimitMb, challenge.XpReward, challenge.CoinReward,
            challenge.StarterCode, challenge.Constraints, challenge.InputFormat, challenge.OutputFormat,
            challenge.SampleInput, challenge.SampleOutput, challenge.Hint,
            !string.IsNullOrWhiteSpace(challenge.Hint), true, challenge.IsPublished, challenge.RequiredLevel,
            challenge.TestCases.OrderBy(t => t.OrderIndex)
                .Select(t => new TestCaseDto(t.Id, t.Input, t.ExpectedOutput, t.OrderIndex))
                .ToList(),
            null,
            false);
    }
}
