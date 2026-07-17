using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record CreateChallengeCommand(
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
    string? StarterCodeEn = null,
    string? Tags = null,
    bool IsBattleOnly = false) : ICommand<ChallengeDetailDto>;

public class CreateChallengeCommandValidator : AbstractValidator<CreateChallengeCommand>
{
    public CreateChallengeCommandValidator()
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

public class CreateChallengeCommandHandler : IRequestHandler<CreateChallengeCommand, ChallengeDetailDto>
{
    private readonly IApplicationDbContext _context;

    public CreateChallengeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChallengeDetailDto> Handle(CreateChallengeCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ChallengeCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeCategory), request.CategoryId);

        var difficulty = await _context.ChallengeDifficulties.FirstOrDefaultAsync(d => d.Id == request.DifficultyId, cancellationToken)
            ?? throw new NotFoundException(nameof(ChallengeDifficulty), request.DifficultyId);

        var normalizedTitle = request.Title.Trim().ToLower();
        var duplicateLevel = await _context.Challenges
            .Where(c => c.Title.ToLower() == normalizedTitle && c.RequiredLevel != request.RequiredLevel)
            .Select(c => (int?)c.RequiredLevel)
            .FirstOrDefaultAsync(cancellationToken);
        if (duplicateLevel is not null)
        {
            throw new ConflictException($"\"{request.Title}\" başlıqlı çağırış artıq Level {duplicateLevel}-də mövcuddur. Hər çağırış yalnız bir səviyyədə istifadə oluna bilər.");
        }

        var challenge = new Challenge
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = category.Id,
            DifficultyId = difficulty.Id,
            TimeLimitMs = request.TimeLimitMs,
            MemoryLimitMb = request.MemoryLimitMb,
            XpReward = request.XpReward,
            CoinReward = request.CoinReward,
            StarterCode = request.StarterCode,
            Constraints = request.Constraints,
            InputFormat = request.InputFormat,
            OutputFormat = request.OutputFormat,
            SampleInput = request.SampleInput,
            SampleOutput = request.SampleOutput,
            Hint = request.Hint,
            IsPublished = request.IsPublished,
            RequiredLevel = request.RequiredLevel,
            TitleEn = request.TitleEn,
            DescriptionEn = request.DescriptionEn,
            ConstraintsEn = request.ConstraintsEn,
            InputFormatEn = request.InputFormatEn,
            OutputFormatEn = request.OutputFormatEn,
            HintEn = request.HintEn,
            StarterCodeEn = request.StarterCodeEn,
            Tags = request.Tags,
            IsBattleOnly = request.IsBattleOnly,
        };

        _context.Challenges.Add(challenge);
        await _context.SaveChangesAsync(cancellationToken);

        return new ChallengeDetailDto(
            challenge.Id, challenge.Title, challenge.Description,
            challenge.CategoryId, category.Name, challenge.DifficultyId, difficulty.Name,
            challenge.TimeLimitMs, challenge.MemoryLimitMb, challenge.XpReward, challenge.CoinReward,
            challenge.StarterCode, challenge.Constraints, challenge.InputFormat, challenge.OutputFormat,
            challenge.SampleInput, challenge.SampleOutput, challenge.Hint,
            !string.IsNullOrWhiteSpace(challenge.Hint), true, challenge.IsPublished, challenge.RequiredLevel,
            [], [], false, Tags: challenge.Tags, IsBattleOnly: challenge.IsBattleOnly);
    }
}
